using System;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support of operations for ASP .NET Core.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configureOptions">Configure options delegate.</param>
        /// <returns>Operations builder.</returns>
        [NotNull]
        public static Builder AddOperationsForAspNetCore(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [CanBeNull] Action<Options> configureOptions = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            var builder = serviceCollection.AddOperations(configuration, configureOptions);

            serviceCollection.ScanAssemblyForImplementations(typeof(ServiceCollectionExtensions).Assembly);

            return builder;
        }
    }
}