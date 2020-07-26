using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" /> with methods configuring internal events infrastructure.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds internal events infrastructure.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        [PublicAPI]
        public static IServiceCollection AddInternalEvents([NotNull] this IServiceCollection serviceCollection)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);

            return serviceCollection;
        }
    }
}
