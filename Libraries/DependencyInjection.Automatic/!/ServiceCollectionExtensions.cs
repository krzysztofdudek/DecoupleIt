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
        ///         Method scans the given assembly for classes marked to be registered automatically.
        ///         Attributes searched for:
        ///         <list type="bullet">
        ///             <item>
        ///                 <term>SingletonAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>TransientAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>ScopedAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>RegisterAsAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>RegisterAsSelfAttribute</term>
        ///             </item>
        ///             <item>
        ///                 <term>RegisterManyTimesAttribute</term>
        ///             </item>
        ///         </list>
        ///         The method of registration of first three attributes is taken by looking at the nearest annotation of base class or implemented interface.
        ///     </para>
        ///     <para>
        ///         Method scans given assembly for classes that can be registered and registers them according to default setting
        ///         or attribute marker.
        ///         By default all registrations made by this method are singletons unless class is marked with
        ///         <see cref="SingletonAttribute" />, <see cref="ScopedAttribute" /> or <see cref="TransientAttribute" />.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly containing types to register.</param>
        /// <param name="ignoredTypes">
        ///     Types that will be ignored within scanning process. Class or interface type can be passed.
        /// </param>
        /// <param name="registerAsManyTypes">Service types that can be registered many times instead of being overridden.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        public static void ScanAssemblyForImplementations(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Assembly assembly,
            [CanBeNull] [ItemNotNull] Type[] ignoredTypes = default,
            [CanBeNull] [ItemNotNull] Type[] registerAsManyTypes = default)
        {
            ignoredTypes        = ignoredTypes ?? new Type[0];
            registerAsManyTypes = registerAsManyTypes ?? new Type[0];

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

                    if (registerManyTimes || registerAsManyTypes.Contains(serviceDescriptor.AsNotNull()
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
            typeof(RegisterManyTimesAttribute),
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
