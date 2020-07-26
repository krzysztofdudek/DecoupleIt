using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Tracing.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds tracing for ASP .NET Core.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configure">Delegate configuring options.</param>
        [NotNull]
        public static IServiceCollection AddTracingForAspNetCore(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [CanBeNull] ConfigureDelegate<TracingOptions> configure = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            serviceCollection.AddTracing(configuration, configure);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);

            return serviceCollection;
        }
    }
}
