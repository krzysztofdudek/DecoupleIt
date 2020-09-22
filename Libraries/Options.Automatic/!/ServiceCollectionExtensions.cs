using System;
using System.Linq;
using System.Reflection;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Scans assembly for options marked with <see cref="ConfigureAttribute" />. Configuration section name is equal to
        ///     namespace where options are located.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection ScanAssemblyForOptions(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Assembly assembly,
            [NotNull] IConfiguration configuration)
        {
            serviceCollection.AddOptions();

            var optionsTypes = assembly.GetTypes()
                                       .Select(x => new
                                       {
                                           type                          = x,
                                           configureAttribute            = x.GetCustomAttribute<ConfigureAttribute>(),
                                           configureAsNamespaceAttribute = x.GetCustomAttribute<ConfigureAsNamespaceAttribute>()
                                       })
                                       .Where(x => x.configureAttribute != null || x.configureAsNamespaceAttribute != null)
                                       .AsCollectionWithNotNullItems();

            foreach (var optionsType in optionsTypes)
            {
                var type                          = optionsType.type.AsNotNull();
                var configureAttribute            = optionsType.configureAttribute;
                var configureAsNamespaceAttribute = optionsType.configureAsNamespaceAttribute;

                var sectionNames = Enumerable.Empty<string>()
                                             .Concat(new[]
                                             {
                                                 (configureAttribute?.ConfigurationSectionName ?? (type.FullName.AsNotNull()
                                                                                                       .EndsWith("Options")
                                                     ? type.FullName.AsNotNull()
                                                           .Substring(type.FullName.AsNotNull()
                                                                          .Length - 7)
                                                     : type.FullName))?.Replace('.', ':')
                                             })
                                             .Concat(new[]
                                             {
                                                 configureAsNamespaceAttribute != null ? type.Namespace : null
                                             })
                                             .Where(x => x != null)
                                             .ToList();

                var configurationSection = (IConfiguration) configuration.GetSection(sectionNames.First());

                serviceCollection.Add(ServiceDescriptor.Singleton(typeof(IOptionsChangeTokenSource<>).MakeGenericType(type),
                                                                  Activator.CreateInstance(typeof(ConfigurationChangeTokenSource<>).MakeGenericType(type),
                                                                                           Microsoft.Extensions.Options.Options.DefaultName,
                                                                                           configurationSection)));

                serviceCollection.Add(ServiceDescriptor.Singleton(typeof(IConfigureOptions<>).MakeGenericType(type),
                                                                  Activator.CreateInstance(
                                                                      typeof(NamedConfigureFromConfigurationOptions<>).MakeGenericType(type),
                                                                      Microsoft.Extensions.Options.Options.DefaultName,
                                                                      configurationSection,
                                                                      new Action<BinderOptions>(_ => { }))));
            }

            return serviceCollection;
        }
    }
}
