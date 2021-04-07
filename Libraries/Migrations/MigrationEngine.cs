using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Migrations
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    internal sealed class MigrationEngine
    {
        public MigrationEngine(
            [NotNull] IEnumerable<IMigration> migrations,
            [NotNull] IOptions<Options> options,
            [NotNull] IUnitOfWorkAccessor unitOfWorkAccessor,
            [NotNull] IHostInformation hostInformation,
            [NotNull] ILogger<MigrationEngine> logger,
            [NotNull] ITracer tracer,
            [CanBeNull] DbContextOptions<MigrationsDbContext> dbContextOptions = default)
        {
            _migrations         = migrations;
            _options            = options.Value!;
            _unitOfWorkAccessor = unitOfWorkAccessor;
            _hostInformation    = hostInformation;
            _logger             = logger;
            _tracer             = tracer;
            _dbContextOptions   = dbContextOptions;
        }

        public async ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return;

            var migrations = GetMigrations();

            if (!migrations.Any())
                return;

            using var span = _tracer.OpenSpan(typeof(MigrationEngine), SpanType.InternalProcess);

            _logger.LogInformation("Migration process {@OperationAction}.", "started");

            try
            {
                await ExecuteMigrationsAsync(migrations, cancellationToken);

                _logger.LogInformation("Migration process {@OperationAction} after {@OperationDuration}ms. All migration have been executed.",
                                       "finished",
                                       span.Duration.Milliseconds);
            }
            catch
            {
                _logger.LogInformation("Migration process {@OperationAction} after {@OperationDuration}ms. NOT ALL migration have been executed.",
                                       "failed",
                                       span.Duration.Milliseconds);

                throw;
            }
        }

        [CanBeNull]
        private readonly DbContextOptions<MigrationsDbContext> _dbContextOptions;

        [NotNull]
        private readonly IHostInformation _hostInformation;

        [NotNull]
        private readonly ILogger<MigrationEngine> _logger;

        [NotNull]
        private readonly IEnumerable<IMigration> _migrations;

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly ITracer _tracer;

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;

        private async Task ExecuteMigrationsAsync(
            [NotNull] IEnumerable<(IMigration migration, MigrationAttribute attribute)> migrations,
            CancellationToken cancellationToken)
        {
            if (_dbContextOptions is null)
                throw new InvalidOperationException("MigrationDbContext is not configured.");

            await using var dbContext = _unitOfWorkAccessor.Get<MigrationsDbContext>();

            if (dbContext.Database!.CurrentTransaction is not null)
                throw new InvalidOperationException("Detected ongoing transaction on the beginning of the migration process. Aborting.");

            foreach (var (migration, attribute) in migrations)
            {
                await using var transaction = await dbContext.Database!.BeginTransactionAsync(cancellationToken)!;

                var existingMigration = await _options.GetMigrations(dbContext,
                                                                     attribute!.Number,
                                                                     migration!.GetType()
                                                                               .FullName!,
                                                                     cancellationToken);

                if (existingMigration is null)
                    continue;

                await ExecuteSingleMigrationAsync(migration!, attribute!, cancellationToken);

                var migrationEntry = new Migration(attribute!.Number,
                                                   migration.GetType()
                                                            .FullName!,
                                                   attribute.Description,
                                                   DateTime.UtcNow,
                                                   _hostInformation.Identifier,
                                                   _hostInformation.Name,
                                                   _hostInformation.Version);

                await dbContext.Migrations.AddAsync(migrationEntry, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction!.CommitAsync(cancellationToken)!;
            }
        }

        private async Task ExecuteSingleMigrationAsync(
            [NotNull] IMigration migration,
            [NotNull] MigrationAttribute attribute,
            CancellationToken cancellationToken)
        {
            var migrationName = migration.GetType()
                                         .FullName!;

            using var upgradeSpan = _tracer.OpenSpan($"Migration({attribute.Number} - {migrationName} - {attribute.Description})", SpanType.InternalProcess);

            try
            {
                _logger.LogInformation("Executing migration upgrade action {@OperationAction}.", "started");

                await migration.UpgradeAsync(cancellationToken);

                _logger.LogInformation("Migration upgrade action execution {@OperationAction} after {@OperationDuration}ms.",
                                       "finished",
                                       upgradeSpan.Duration.Milliseconds);
            }
            catch (Exception upgradeException)
            {
                _logger.LogError(upgradeException,
                                 "Migration upgrade action execution {@OperationAction} after {@OperationDuration}ms.",
                                 "failed",
                                 upgradeSpan.Duration.Milliseconds);

                using var downgradeSpan =
                    _tracer.OpenSpan($"Migration({attribute.Number} - {migrationName} - {attribute.Description})", SpanType.InternalProcess);

                try
                {
                    _logger.LogInformation("Executing migration downgrade action {@OperationAction}.", "started");

                    await migration.DowngradeAsync(cancellationToken);

                    _logger.LogInformation("Migration downgrade action execution {@OperationAction} after {@OperationDuration}ms.",
                                           "finished",
                                           upgradeSpan.Duration.Milliseconds);
                }
                catch (Exception downgradeException)
                {
                    _logger.LogCritical(downgradeException,
                                        "Migration downgrade action execution {@OperationAction} after {@OperationDuration}ms.",
                                        "failed",
                                        downgradeSpan.Duration.Milliseconds);

                    throw new MigrationFailed(downgradeException);
                }

                throw new MigrationFailed(upgradeException);
            }
        }

        [NotNull]
        private IReadOnlyCollection<(IMigration migration, MigrationAttribute attribute)> GetMigrations()
        {
            var migrations = _migrations.Select(x => (migration: x, attribute: x!.GetType()
                                                                                 .GetCustomAttribute<MigrationAttribute>()))
                                        .OrderBy(x => x.attribute?.Number)
                                        .ToList();

            foreach (var migration in migrations.Where(migration => migration.attribute is null))
                throw new MigrationFailed($"Migration \"{migration.GetType().FullName}\" has no Migration attribute assigned.");

            return migrations;
        }
    }
}
