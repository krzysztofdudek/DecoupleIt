using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds tracing.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddTracing([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            serviceCollection.ScanAssemblyForImplementations(ThisAssembly);
            serviceCollection.ScanAssemblyForOptions(ThisAssembly, configuration);

            return new Builder(serviceCollection, configuration);
        }

        [NotNull]
        private static readonly Assembly ThisAssembly = typeof(ServiceCollectionExtensions).Assembly;
    }
}
