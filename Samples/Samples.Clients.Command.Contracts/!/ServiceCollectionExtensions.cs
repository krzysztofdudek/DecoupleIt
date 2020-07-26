using GS.DecoupleIt.HttpAbstraction;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Samples.Clients.Command.Contracts.Services;

namespace Samples.Clients.Command.Contracts
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds services exposed by Samples.Clients.Command service.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <returns></returns>
        [NotNull]
        public static IServiceCollection AddClientsCommandServices([NotNull] this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClientService<IClients>();
            serviceCollection.AddHttpClientService<IClientsBaskets>();

            return serviceCollection;
        }
    }
}
