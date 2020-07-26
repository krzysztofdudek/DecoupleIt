using GS.DecoupleIt.HttpAbstraction;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Samples.Documents.Command.Contracts.Services;

namespace Samples.Documents.Command.Contracts
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds services exposed by Samples.Documents.Command service.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <returns></returns>
        [NotNull]
        public static IServiceCollection AddDocumentsCommandServices([NotNull] this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClientService<IDocuments>();

            return serviceCollection;
        }
    }
}
