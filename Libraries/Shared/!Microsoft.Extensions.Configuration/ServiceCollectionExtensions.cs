using System.Collections.Generic;
using System.Linq;
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
    }
}
