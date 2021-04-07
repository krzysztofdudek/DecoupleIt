using System;
using GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GS.DecoupleIt.Migrations
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    public sealed class MigrationsDbContext : UnitOfWorkDbContext
    {
        [NotNull]
        [ItemNotNull]
        public DbSet<Migration> Migrations { get; [UsedImplicitly] set; }

        public MigrationsDbContext(
            [NotNull] DbContextOptions<MigrationsDbContext> options,
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] Action<ModelBuilder> configureModelBuilder) : base(options)
        {
            _serviceProvider       = serviceProvider;
            _configureModelBuilder = configureModelBuilder;
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseApplicationServiceProvider(_serviceProvider);
        }

        protected override void OnModelCreating([NotNull] ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var migration = modelBuilder.Entity<Migration>();

            migration!.HasKey(x => x.Id);
            migration.Property(x => x.Name)!.HasMaxLength(300);
            migration.Property(x => x.Description)!.HasMaxLength(2000);
            migration.Property(x => x.HostName)!.HasMaxLength(300);
            migration.Property(x => x.HostVersion)!.HasMaxLength(50);
            migration.Property(x => x.ExecutedOn);
            migration.Property(x => x.HostIdentifier);

            migration.HasIndex(x => new
            {
                x.Number,
                x.Name
            });

            _configureModelBuilder?.Invoke(modelBuilder);
        }

        [CanBeNull]
        private readonly Action<ModelBuilder> _configureModelBuilder;

        [NotNull]
        private readonly IServiceProvider _serviceProvider;
    }
}
