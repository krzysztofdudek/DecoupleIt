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
                                       .Select(x => (x, x.GetCustomAttribute<ConfigureAttribute>()))
                                       .Where(x => x.Item2 != null)
                                       .ToArray();

            foreach (var (type, attribute) in optionsTypes)
            {
                var attributeSectionName = attribute.AsNotNull()
                                                    .ConfigurationSectionName?.Replace(".", ":");

                var typeSectionName = type.AsNotNull()
                                          .FullName.AsNotNull()
                                          .Replace(".", ":");

                if (typeSectionName.EndsWith("Options"))
                    typeSectionName = typeSectionName.Substring(0, typeSectionName.Length - "Options".Length);

                var configurationSection = (IConfiguration) configuration.GetSection(attributeSectionName ?? typeSectionName);

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
