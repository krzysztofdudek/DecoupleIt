using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Builder class of unit of work extensions.
    /// </summary>
    [PublicAPI]
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }

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
