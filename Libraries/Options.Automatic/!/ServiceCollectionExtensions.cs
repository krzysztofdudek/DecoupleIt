using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                                           type = x,
                                           configureAttributes = x.GetCustomAttributes<ConfigureAttribute>()
                                                                  .ToList()
                                                                  .AsReadOnly(),
                                           configureAsNamespaceAttributes = x.GetCustomAttributes<ConfigureAsNamespaceAttribute>()
                                                                             .ToList()
                                                                             .AsReadOnly()
                                       })
                                       .Where(x => x.configureAttributes.Any() || x.configureAsNamespaceAttributes.Any())
                                       .AsCollectionWithNotNullItems();

            foreach (var optionsType in optionsTypes)
            {
                var type                           = optionsType.type.AsNotNull();
                var configureAttributes            = optionsType.configureAttributes.AsNotNull();
                var configureAsNamespaceAttributes = optionsType.configureAsNamespaceAttributes.AsNotNull();

                var attributes = configureAttributes.Cast<IConfigureAttribute>()
                                                    .Concat(configureAsNamespaceAttributes)
                                                    .OrderBy(x => x.AsNotNull()
                                                                   .Priority)
                                                    .ToList()
                                                    .AsReadOnly();

                var sectionNames = GetSectionNames(type, attributes);

                RegisterSections(serviceCollection,
                                 configuration,
                                 sectionNames,
                                 type);
            }

            return serviceCollection;
        }

        [NotNull]
        private static string GetOptionsPath([NotNull] Type type)
        {
            return type.FullName.AsNotNull()
                       .EndsWith("Options")
                ? type.FullName.AsNotNull()
                      .Substring(0,
                                 type.FullName.AsNotNull()
                                     .Length - 7)
                : type.FullName.AsNotNull();
        }

        [NotNull]
        [ItemCanBeNull]
        private static IEnumerable<string> GetSectionNames([NotNull] Type type, [NotNull] [ItemNotNull] IEnumerable<IConfigureAttribute> configureAttributes)
        {
            var sectionNames = new List<string>();

            foreach (var attribute in configureAttributes)
                switch (attribute)
                {
                    case ConfigureAttribute configureAttribute when configureAttribute.ConfigurationSectionName != null:
                        sectionNames.Add(configureAttribute.ConfigurationSectionName.Replace('.', ':'));

                        break;
                    case ConfigureAttribute configureAttribute when configureAttribute.ConfigurationSectionName == null:
                        sectionNames.Add(GetOptionsPath(type)
                                             .Replace('.', ':'));

                        break;
                    case ConfigureAsNamespaceAttribute _ when type.Namespace == null:
                        throw new InvalidOperationException("Cannot register options by namespace by type without it.");

                    case ConfigureAsNamespaceAttribute _:
                        sectionNames.Add(type.Namespace.Replace('.', ':'));

                        break;
                }

            return sectionNames.Distinct()
                               .ToList()
                               .AsReadOnly();
        }

        private static void RegisterOptions<TOptions>(
            [NotNull] IServiceCollection serviceCollection,
            [NotNull] IReadOnlyCollection<IConfiguration> configurations)
            where TOptions : class
        {
            var baseConfiguration = configurations.First();

            serviceCollection.Configure<TOptions>(baseConfiguration);

            foreach (var configuration in configurations.Skip(1))
                serviceCollection.PostConfigure<TOptions>(options => { configuration.Bind(options); });
        }

        private static void RegisterSections(
            [NotNull] IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [NotNull] IEnumerable<string> sectionNames,
            [NotNull] Type type)
        {
            var methodInfo = typeof(ServiceCollectionExtensions).GetMethod(nameof(RegisterOptions), BindingFlags.Static | BindingFlags.NonPublic)
                                                                .AsNotNull()
                                                                .MakeGenericMethod(type)
                                                                .AsNotNull();

            var configurationSections = (IReadOnlyCollection<IConfiguration>) sectionNames.Select(x => (IConfiguration) configuration.GetSection(x))
                                                                                          .ToList();

            methodInfo.Invoke(null,
                              new object[]
                              {
                                  serviceCollection,
                                  configurationSections
                              });
        }
    }
}
