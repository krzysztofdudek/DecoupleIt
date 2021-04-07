using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Migrations
{
    /// <summary>
    ///     Builder for migrations.
    /// </summary>
    [PublicAPI]
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }

        /// <summary>
        ///     Configures DbContext.
        /// </summary>
        /// <param name="configureBuilderDelegate">Configure builder delegate.</param>
        /// <param name="configureModelBuilderDelegate">Configure <see cref="ModelBuilder" /> delegate.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public Builder ConfigureDbContext(
            [NotNull] Action<DbContextOptionsBuilder<MigrationsDbContext>> configureBuilderDelegate,
            [CanBeNull] Action<ModelBuilder> configureModelBuilderDelegate = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(configureBuilderDelegate), configureBuilderDelegate);

            var builder = new DbContextOptionsBuilder<MigrationsDbContext>();

            configureBuilderDelegate(builder);

            ServiceCollection.AddSingleton(builder.Options!);

            if (configureModelBuilderDelegate is not null)
                ServiceCollection.AddSingleton(new ModelBuilderConfigurator(configureModelBuilderDelegate));

            return this;
        }

        /// <summary>
        ///     Configures options.
        /// </summary>
        /// <param name="configureOptionsDelegate">Configure options delegate.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public Builder ConfigureOptions([NotNull] Action<Options> configureOptionsDelegate)
        {
            ContractGuard.IfArgumentIsNull(nameof(configureOptionsDelegate), configureOptionsDelegate);

            ServiceCollection.PostConfigure(configureOptionsDelegate);

            return this;
        }
    }
}
