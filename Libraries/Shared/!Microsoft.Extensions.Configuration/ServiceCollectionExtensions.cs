using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Configures options as dictionary.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configurationSection">Configuration section.</param>
        /// <typeparam name="TOptions">Options type.</typeparam>
        /// <returns>Service collection.</returns>
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static IServiceCollection ConfigureDictionary<TOptions>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfigurationSection configurationSection)
            where TOptions : class, IDictionary<string, string>
        {
            var values = configurationSection.GetChildren()
                                             .ToList();

            serviceCollection.Configure<TOptions>(x => values.ForEach(v => x.Add(v.Key, v.Value)));

            return serviceCollection;
        }

        /// <summary>
        ///     Configures options.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configure">Configure method.</param>
        /// <typeparam name="TOptions">Options type.</typeparam>
        public static void ConfigureOptionsAndPostConfigure<TOptions>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfigurationSection configuration,
            [CanBeNull] ConfigureDelegate<TOptions> configure = default)
            where TOptions : class, new()
        {
            serviceCollection.Configure<TOptions>(configuration);

            serviceCollection.PostConfigure<TOptions>(options => configure?.Invoke(options.AsNotNull()));
        }

        /// <summary>
        ///     Configures options as dictionary.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configure">Configure method.</param>
        /// <typeparam name="TOptions">Options type.</typeparam>
        public static void ConfigureOptionsDictionaryAndPostConfigure<TOptions>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfigurationSection configuration,
            [CanBeNull] ConfigureDelegate<TOptions> configure = default)
            where TOptions : class, IDictionary<string, string>
        {
            serviceCollection.ConfigureDictionary<TOptions>(configuration);

            serviceCollection.PostConfigure<TOptions>(options => configure?.Invoke(options.AsNotNull()));
        }

        /// <summary>
        ///     Gets options.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configure">Configure method.</param>
        /// <typeparam name="TOptions">Options type.</typeparam>
        /// <returns>Options instance.</returns>
        [NotNull]
        public static TOptions GetOptions<TOptions>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfigurationSection configuration,
            [CanBeNull] ConfigureDelegate<TOptions> configure = default)
            where TOptions : class, new()
        {
            var options = configuration.Get<TOptions>() ?? new TOptions();

            configure?.Invoke(options);

            return options;
        }
    }
}
