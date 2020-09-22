using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
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
        [NotNull]
        public static IServiceCollection AddTracing([NotNull] this IServiceCollection serviceCollection)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            var assembly = typeof(ServiceCollectionExtensions).Assembly;

            serviceCollection.ScanAssemblyForImplementations(assembly);

            return serviceCollection;
        }
    }
}
