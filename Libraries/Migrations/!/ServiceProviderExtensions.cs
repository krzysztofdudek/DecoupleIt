using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Extends <see cref="IServiceProvider" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Executed migrations registered in the DI container (<see cref="IMigration" /> service).
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public static void ExecuteMigrations([NotNull] this IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService<DbContextOptions<MigrationsDbContext>>() is null)
                return;

            using var dbContext = serviceProvider.GetRequiredService<IUnitOfWorkAccessor>()!.Get<MigrationsDbContext>();

            dbContext.Database!.EnsureCreated();

            serviceProvider.GetRequiredService<MigrationEngine>()!.ExecuteAsync()
                           .GetAwaiter()
                           .GetResult();
        }

        /// <summary>
        ///     Executed migrations registered in the DI container (<see cref="IMigration" /> service).
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public static async ValueTask ExecuteMigrationsAsync([NotNull] this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            if (serviceProvider.GetService<DbContextOptions<MigrationsDbContext>>() is null)
                return;

            await using var dbContext = serviceProvider.GetRequiredService<IUnitOfWorkAccessor>()!.Get<MigrationsDbContext>();

            await dbContext.Database!.EnsureCreatedAsync(cancellationToken)!;

            await serviceProvider.GetRequiredService<MigrationEngine>()!.ExecuteAsync(cancellationToken);
        }
    }
}
