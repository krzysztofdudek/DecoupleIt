using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
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
        [NotNull]
        public static IServiceCollection AddTracingForAspNetCore([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

            var assembly = typeof(ServiceCollectionExtensions).Assembly;

            serviceCollection.AddTracing(configuration);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);
            serviceCollection.ScanAssemblyForOptions(assembly, configuration);

            return serviceCollection;
        }
    }
}
