using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Class contains extensions for <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds a new service descriptor if there is no other with the same <see cref="ServiceDescriptor.ServiceType" />. If
        ///     such exists,
        ///     all that entries are removed and replaced with passed service descriptor.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="serviceDescriptor">New service descriptor.</param>
        public static void AddOrReplace([NotNull] this IServiceCollection serviceCollection, [NotNull] ServiceDescriptor serviceDescriptor)
        {
            foreach (var existingServiceDescriptor in serviceCollection.Where(x => x.AsNotNull()
                                                                                    .ServiceType == serviceDescriptor.ServiceType)
                                                                       .ToList())
                serviceCollection.Remove(existingServiceDescriptor);

            serviceCollection.Add(serviceDescriptor);
        }

        /// <summary>
        ///     <para>
        ///         Method scans given assembly for classes that can be registered and registers them according to default setting
        ///         or attribute marker.
        ///         By default all registrations made by this method are singletons unless class is marked with
        ///         <see cref="SingletonAttribute" />, <see cref="ScopedAttribute" /> or <see cref="TransientAttribute" />.
        ///     </para>
        ///     <para>
        ///         Adds or replaces services located in given assembly that:
        ///         <list type="bullet">
        ///             <item>
        ///                 <term>are classes</term>
        ///             </item>
        ///             <item>
        ///                 <term>have at least one public constructor</term>
        ///             </item>
        ///             <item>
        ///                 <term>are not abstract nor static</term>
        ///             </item>
        ///             <item>
        ///                 <term>are not excluded with ignored types</term>
        ///             </item>
        ///             <item>
        ///                 <term>are not annotated with DoNotRegisterAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>
        ///                     implements at least one interface or is annotated with RegisterAsSelfAttribute or
        ///                     RegisterAsAttribute
        ///                 </term>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly containing types to register.</param>
        /// <param name="ignoredTypes">
        ///     Ignored types that would be not taken for registration process. Type or interface can be
        ///     passed.
        /// </param>
        /// <param name="notOverridableTypes">Types that should not be overriden.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        public static void ScanAssemblyForImplementations(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Assembly assembly,
            [CanBeNull] [ItemNotNull] Type[] ignoredTypes = default,
            [CanBeNull] [ItemNotNull] Type[] notOverridableTypes = default)
        {
            ignoredTypes        = ignoredTypes ?? new Type[0];
            notOverridableTypes = notOverridableTypes ?? new Type[0];

            var types = GetTypesToRegister(assembly, ignoredTypes);

            foreach (var implementationType in types)
            {
                RegisterImplementation(serviceCollection, implementationType);

                IEnumerable<Type> serviceTypes = new Type[0];

                var registerAsSelf = implementationType.GetCustomAttribute<RegisterAsSelfAttribute>() != null;

                var registerAs = implementationType.GetCustomAttributes<RegisterAsAttribute>()
                                                   .ToList();

                if (!registerAsSelf && !registerAs.Any())
                    serviceTypes = implementationType.GetAllInterfaces()
                                                     .Concat(implementationType.GetAllBaseTypes())
                                                     .Except(AlwaysIgnoredTypes)
                                                     .Where(serviceType => !ignoredTypes.Any(serviceType.AsNotNull()
                                                                                                        .InheritsOrImplements));
                else if (registerAs.Any())
                    serviceTypes = serviceTypes.Concat(registerAs.Select(x => x.AsNotNull()
                                                                               .ServiceType));

                var serviceDescriptors = serviceTypes
                                         .Select(serviceType => ServiceDescriptor.Transient(serviceType, x => x.GetRequiredService(implementationType)))
                                         .ToList();

                foreach (var serviceDescriptor in serviceDescriptors)
                {
                    var registerManyTimes = serviceDescriptor.AsNotNull()
                                                             .ServiceType.AsNotNull()
                                                             .GetCustomAttributes(typeof(RegisterManyTimesAttribute), true)
                                                             .Any() || serviceDescriptor.AsNotNull()
                                                                                        .ServiceType.AsNotNull()
                                                                                        .GetAllInterfaces()
                                                                                        .Any(x => x.GetCustomAttributes(typeof(RegisterManyTimesAttribute),
                                                                                                       true)
                                                                                                   .Any());

                    if (registerManyTimes || notOverridableTypes.Contains(serviceDescriptor.AsNotNull()
                                                                                           .ServiceType))
                        serviceCollection.Add(serviceDescriptor);
                    else
                        serviceCollection.AddOrReplace(serviceDescriptor.AsNotNull());
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        private static Type[] _attributesMeaningAuto =
        {
            typeof(RegisterAsAttribute),
            typeof(RegisterAsSelfAttribute),
            typeof(TransientAttribute),
            typeof(SingletonAttribute),
            typeof(ScopedAttribute)
        };

        [NotNull]
        [ItemCanBeNull]
        private static readonly Type[] AlwaysIgnoredTypes =
        {
            typeof(object)
        };

        [NotNull]
        [ItemNotNull]
        private static Type[] GetTypesToRegister([NotNull] Assembly assembly, [NotNull] [ItemNotNull] Type[] ignoredTypes)
        {
            return assembly.GetTypes()
                           .Where(x => ValidateType(x, ignoredTypes))
                           .ToArray();
        }

        private static void RegisterImplementation([NotNull] IServiceCollection serviceCollection, [NotNull] Type type)
        {
            var attribute = type.GetTheMostImportantLifetimeAttribute();

            ServiceDescriptor serviceDescriptor;

            switch (attribute)
            {
                case TransientAttribute _:
                    serviceDescriptor = ServiceDescriptor.Transient(type, type);

                    break;
                case ScopedAttribute _:
                    serviceDescriptor = ServiceDescriptor.Scoped(type, type);

                    break;
                default:
                    serviceDescriptor = ServiceDescriptor.Singleton(type, type);

                    break;
            }

            serviceCollection.AddOrReplace(serviceDescriptor.AsNotNull());
        }

        private static bool ValidateType([NotNull] Type type, [NotNull] Type[] ignoredTypes)
        {
            if (!(type.IsClass && type.GetConstructors()
                                      .Any() && !type.IsAbstract && !ignoredTypes.Any(type.InheritsOrImplements)))
                return false;

            var attributesTypes = type.GetAllBaseTypes()
                                      .Concat(type.GetAllInterfaces())
                                      .Concat(new[]
                                      {
                                          type
                                      })
                                      .Select(x => x.AsNotNull()
                                                    .GetCustomAttributes())
                                      .SelectMany(x => x)
                                      .Distinct()
                                      .Select(x => x.AsNotNull()
                                                    .GetType());

            if (!_attributesMeaningAuto.Intersect(attributesTypes)
                                       .Any())
                return false;

            return true;
        }
    }
}
