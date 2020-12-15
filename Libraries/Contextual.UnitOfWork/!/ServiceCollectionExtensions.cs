using System;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" /> with methods enabling contextual unit of work.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds contextual unit of work functionality and access to <see cref="IUnitOfWorkAccessor" />.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddContextualUnitOfWork([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

            var assembly = typeof(ServiceCollectionExtensions).Assembly;

            serviceCollection.ScanAssemblyForImplementations(assembly);
            serviceCollection.ScanAssemblyForOptions(assembly, configuration);

            return new Builder(serviceCollection, configuration);
        }

        /// <summary>
        ///     Registers factory of unit of work. It have to be used when unit of work is registered as transient.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="factory">Factory method.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection AddUnitOfWorkFactory<TUnitOfWork>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Func<IServiceProvider, TUnitOfWork> factory)
            where TUnitOfWork : class, IUnitOfWork
        {
            serviceCollection.AddTransient<Func<TUnitOfWork>>(x => () => factory(x));

            return serviceCollection;
        }
    }
}
