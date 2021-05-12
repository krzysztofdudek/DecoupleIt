using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.AspNetCore.Service.Tracing
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
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddTracingForAspNetCore([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

            var assembly = typeof(ServiceCollectionExtensions).Assembly;

            var builder = serviceCollection.AddTracing(configuration);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);
            serviceCollection.ScanAssemblyForOptions(assembly, configuration);

            return builder;
        }
    }
}
