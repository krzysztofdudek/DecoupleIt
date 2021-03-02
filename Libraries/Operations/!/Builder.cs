using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Builder for operations.
    /// </summary>
    [PublicAPI]
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }

        /// <summary>
        ///     Configures options.
        /// </summary>
        /// <param name="configureOptions">Configure options delegate.</param>
        /// <returns>This builder.</returns>
        [NotNull]
        public Builder WithConfiguration([NotNull] Action<Options> configureOptions)
        {
            ServiceCollection.PostConfigure(configureOptions);

            return this;
        }
    }
}
