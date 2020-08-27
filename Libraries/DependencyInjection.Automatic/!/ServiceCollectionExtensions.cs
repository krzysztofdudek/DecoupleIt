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
        /// <param name="namespace">Namespace that will be scanned within the assembly.</param>
        /// <param name="ignoredTypes">
        ///     Types that will be ignored within scanning process. Class or interface type can be passed.
        /// </param>
        /// <param name="ignoredBaseTypes">
        ///     Types that will exclude any type from registration if inherits or implements it.
        /// </param>
        /// <param name="registerAsManyTypes">Service types that can be registered many times instead of being overridden.</param>
        public static void ScanAssemblyForImplementations(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Assembly assembly,
            [CanBeNull] string @namespace = default,
            [CanBeNull] [ItemNotNull] Type[] ignoredTypes = default,
            [CanBeNull] [ItemNotNull] Type[] ignoredBaseTypes = default,
            [CanBeNull] [ItemNotNull] Type[] registerAsManyTypes = default)
        {
            ignoredTypes        ??= new Type[0];
            ignoredBaseTypes    ??= new Type[0];
            registerAsManyTypes ??= new Type[0];

            var types = GetTypesToRegister(assembly,
                                           @namespace,
                                           ignoredTypes,
                                           ignoredBaseTypes);

            foreach (var implementationType in types)
            {
                RegisterImplementation(serviceCollection, implementationType);

                var serviceDescriptors = GetImplementationServices(implementationType, ignoredTypes);

                foreach (var (serviceDescriptor, registerManyTimes) in serviceDescriptors)
                {
                    var descriptor = serviceDescriptor.AsNotNull();

                    if (registerManyTimes || registerAsManyTypes.Contains(descriptor.ServiceType))
                        serviceCollection.Add(descriptor);
                    else
                        serviceCollection.AddOrReplace(descriptor);
                }
            }
        }

        [NotNull]
        [ItemNotNull]
        private static Type[] _attributesMeaningAuto =
        {
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
        private static IEnumerable<(ServiceDescriptor ServiceDescriptor, bool RegisterManyTimes)> GetImplementationServices(
            [NotNull] Type implementationType,
            [NotNull] [ItemNotNull] Type[] ignoredTypes)
        {
            return implementationType.GetAllInterfaces()
                                     .Concat(implementationType.GetAllBaseTypes())
                                     .Distinct()
                                     .Except(AlwaysIgnoredTypes)
                                     .Except(ignoredTypes)
                                     .Select(serviceType => ServiceDescriptor.Transient(serviceType, x => x.GetRequiredService(implementationType))
                                                                             .AsNotNull())
                                     .Select(x => (ServiceDescriptor: x, RegisterManyTimes: x.ServiceType.AsNotNull()
                                                                                             .GetCustomAttributes<RegisterManyTimesAttribute>(false)
                                                                                             .Any()))
                                     .ToList()
                                     .AsCollectionWithNotNullItems();
        }

        [NotNull]
        [ItemNotNull]
        private static Type[] GetTypesToRegister(
            [NotNull] Assembly assembly,
            [CanBeNull] string @namespace,
            [NotNull] [ItemNotNull] Type[] ignoredTypes,
            [NotNull] [ItemNotNull] Type[] ignoredBaseTypes)
        {
            IEnumerable<Type> types = assembly.GetTypes();

            if (!string.IsNullOrWhiteSpace(@namespace))
                types = types.Where(x => x.Namespace?.StartsWith(@namespace) == true);

            return types.Where(x => ValidateType(x, ignoredTypes, ignoredBaseTypes))
                        .ToArray();
        }

        private static void RegisterImplementation([NotNull] IServiceCollection serviceCollection, [NotNull] Type type)
        {
            var serviceDescriptor = type.GetTheMostImportantLifetimeAttribute() switch
            {
                TransientAttribute _ => ServiceDescriptor.Transient(type, type),
                ScopedAttribute _    => ServiceDescriptor.Scoped(type, type),
                _                    => ServiceDescriptor.Singleton(type, type)
            };

            serviceCollection.AddOrReplace(serviceDescriptor.AsNotNull());
        }

        private static bool ValidateType([NotNull] Type type, [NotNull] Type[] ignoredTypes, [NotNull] [ItemNotNull] Type[] ignoredBaseTypes)
        {
            if (!(type.IsClass && type.GetConstructors()
                                      .Any() && !type.IsAbstract && ignoredTypes.All(x => x != type) && !ignoredBaseTypes.Any(type.InheritsOrImplements)))
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

            return _attributesMeaningAuto.Intersect(attributesTypes)
                                         .Any();
        }
    }
}
