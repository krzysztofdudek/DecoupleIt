using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.InternalEvents.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support of internal events for ASP .NET Core.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection AddInternalEventsForAspNetCore([NotNull] this IServiceCollection serviceCollection)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            serviceCollection.AddInternalEvents();

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);

            return serviceCollection;
        }
    }
}
