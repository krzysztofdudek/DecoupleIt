using System.Reflection;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
            serviceCollection.ScanAssemblyForHttpClients(ThisAssembly);

            return serviceCollection;
        }

        [NotNull]
        private static Assembly ThisAssembly => typeof(ServiceCollectionExtensions).Assembly.AsNotNull();
    }
}
