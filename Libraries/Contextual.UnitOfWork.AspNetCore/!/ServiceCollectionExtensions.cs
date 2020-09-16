using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support of contextual unit of work for ASP .NET Core.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        /// <typeparam name="TUnitOfWork">Unit of work type.</typeparam>
        [NotNull]
        public static IServiceCollection AddContextualUnitOfWorkForAspNetCore<TUnitOfWork>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration)
            where TUnitOfWork : class, IUnitOfWork
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

            serviceCollection.AddContextualUnitOfWork(configuration);

            serviceCollection.AddSingleton<UnitOfWorkContextMiddleware<TUnitOfWork>>();

            return serviceCollection;
        }
    }
}
