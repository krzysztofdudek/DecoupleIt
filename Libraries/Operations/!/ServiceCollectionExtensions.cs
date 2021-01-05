using System;
using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support of operations.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configureOptions">Configure options delegate.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddOperations(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [CanBeNull] Action<Options> configureOptions = default)
        {
            serviceCollection.ScanAssemblyForImplementations(ThisAssembly);
            serviceCollection.ScanAssemblyForOptions(ThisAssembly, configuration);

            if (configureOptions is not null)
                serviceCollection.PostConfigure(configureOptions);

            return new Builder(serviceCollection, configuration);
        }

        [NotNull]
        private static readonly Assembly ThisAssembly = typeof(ServiceCollectionExtensions).Assembly;
    }
}
