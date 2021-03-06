using System;
using System.Collections.Generic;
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
    ///     Extends <see cref="IServiceCollection" /> with methods allowing to automatically configure options using attributes.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Method automatically configures all options classes annotated with:<br />
        ///     - <see cref="ConfigureAttribute" /><br />
        ///     - <see cref="ConfigureAsNamespaceAttribute" /><br />
        ///     , with adequate configuration path. Additionally <see cref="ConfigurePropertyAttribute" /> is taken into consideration of mapping properties.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly to scan.</param>
        /// <param name="configuration">Base configuration section to be a root of automapping.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection ScanAssemblyForOptions(
            [NotNull] [ItemNotNull] this IServiceCollection serviceCollection,
            [NotNull] Assembly assembly,
            [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(assembly), assembly);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

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

                // If options are already registered to be configured, skip it.
                if (serviceCollection.Any(x => x.ServiceType == typeof(IConfigureOptions<>).MakeGenericType(type)))
                    continue;

                RegisterSections(serviceCollection,
                                 configuration,
                                 sectionNames,
                                 type);

                RegisterPostConfigurationForProperties(serviceCollection, configuration, type);
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
            static string TransformNameToPath(string name)
            {
                return name.AsNotNull()
                           .Replace('.', ':')
                           .Replace('+', ':');
            }

            var sectionNames = new List<string>();

            foreach (var attribute in configureAttributes)
                switch (attribute)
                {
                    case ConfigureAttribute {ConfigurationPath: { }} configureAttribute:
                        sectionNames.Add(TransformNameToPath(configureAttribute.ConfigurationPath));

                        break;
                    case ConfigureAttribute {ConfigurationPath: null}:
                        sectionNames.Add(TransformNameToPath(GetOptionsPath(type)));

                        break;
                    case ConfigureAsNamespaceAttribute _ when type.Namespace == null:
                        throw new InvalidOperationException("Cannot register options by namespace by type without it.");

                    case ConfigureAsNamespaceAttribute _:
                        sectionNames.Add(TransformNameToPath(type.Namespace));

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

        private static void RegisterPostConfigurationForProperties(
            [NotNull] IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [NotNull] Type type)
        {
            var propertiesWithCustomConfiguration = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                        .Select(property => (property, attributes: property.GetCustomAttributes<ConfigurePropertyAttribute>()
                                                                                 .ToList()
                                                                                 .AsEnumerable()))
                                                        .Where(x => x.attributes.AsNotNull()
                                                                     .Any());

            foreach (var (property, attributes) in propertiesWithCustomConfiguration)
            {
                var methodInfo = typeof(ServiceCollectionExtensions)
                                 .GetMethod(nameof(RegisterPostConfigureForProperty), BindingFlags.Static | BindingFlags.NonPublic)
                                 .AsNotNull()
                                 .MakeGenericMethod(type)
                                 .AsNotNull();

                methodInfo.Invoke(null,
                                  new object[]
                                  {
                                      serviceCollection,
                                      configuration,
                                      property,
                                      attributes
                                  });
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        private static void RegisterPostConfigureForProperty<TOptions>(
            [NotNull] IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [NotNull] PropertyInfo property,
            [NotNull] [ItemNotNull] IEnumerable<ConfigurePropertyAttribute> attributes)
            where TOptions : class
        {
            serviceCollection.PostConfigure<TOptions>(options =>
            {
                foreach (var attribute in attributes.OrderBy(x => x.Priority)
                                                    .AsCollectionWithNotNullItems())
                {
                    var configurationSection = configuration.GetSection(attribute.ConfigurationPath);

                    if (property.PropertyType.GetTypeInfo()
                                .AsNotNull()
                                .IsSimple())
                    {
                        if (configurationSection?.Value is null)
                        {
                            if (!attribute.AssignNull)
                                continue;

                            property.SetValue(options, null);

                            continue;
                        }

                        var propertyTypedValue = Convert.ChangeType(configurationSection.Value, property.PropertyType);

                        property.SetValue(options, propertyTypedValue);

                        continue;
                    }

                    if (configurationSection is null)
                    {
                        if (!attribute.AssignNull)
                            continue;

                        property.SetValue(options, null);

                        continue;
                    }

                    if (property.PropertyType.InheritsOrImplements(typeof(ICollection<>)))
                    {
                        var elements = configurationSection.GetChildren()
                                                           .AsNotNull()
                                                           .AsCollectionWithNotNullItems()
                                                           .Select(x => (key: Convert.ToInt32(x.Key), value: x.Value))
                                                           .OrderBy(x => x.key)
                                                           .Select(x => x.value);

                        var propertyValue = property.GetValue(options) ?? Activator.CreateInstance(property.PropertyType);

                        foreach (var element in elements)
                            property.PropertyType.GetMethod("Add")
                                    .AsNotNull()
                                    .Invoke(propertyValue,
                                            new object[]
                                            {
                                                element
                                            });

                        property.SetValue(options, propertyValue);
                    }
                    else
                    {
                        var propertyValue = property.GetValue(options) ?? Activator.CreateInstance(property.PropertyType);

                        configurationSection.Bind(propertyValue);

                        property.SetValue(options, propertyValue);
                    }
                }
            });
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
