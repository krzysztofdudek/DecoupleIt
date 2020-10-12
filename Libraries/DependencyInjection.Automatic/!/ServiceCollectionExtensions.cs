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
        ///         </list>
        ///         The method of registration of first three attributes is taken by looking at the nearest annotation of base
        ///         class or implemented interface.
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

                foreach (var entry in serviceDescriptors)
                    if (entry.RegisterManyTimes || registerAsManyTypes.Contains(entry.ServiceDescriptor.ServiceType))
                    {
                        serviceCollection.Add(entry.ServiceDescriptor);
                        serviceCollection.Add(entry.FactoryServiceDescriptor);
                    }
                    else
                    {
                        serviceCollection.AddOrReplace(entry.ServiceDescriptor);
                        serviceCollection.AddOrReplace(entry.FactoryServiceDescriptor);
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
        private static ServiceDescriptor CreateFactoryServiceDescriptor([NotNull] Type instanceType, [NotNull] Type serviceType)
        {
            return (ServiceDescriptor) typeof(ServiceCollectionExtensions)
                                       .GetMethod(nameof(CreateFactoryServiceDescriptorTyped), BindingFlags.NonPublic | BindingFlags.Static)
                                       .AsNotNull()
                                       .MakeGenericMethod(instanceType, serviceType)
                                       .Invoke(null, null)
                                       .AsNotNull();
        }

        private static ServiceDescriptor CreateFactoryServiceDescriptorTyped<TInstanceType, TServiceType>()
        {
            var type = typeof(TInstanceType);

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            if (constructors.Length > 1)
                throw new InvalidOperationException($"Ambiguous constructor for {type.FullName}.");

            var constructor = constructors.Single()
                                          .AsNotNull();

            var parameters = constructor.GetParameters();

            return ServiceDescriptor.Transient(serviceProvider =>
            {
                return new Func<TServiceType>(() =>
                {
                    var parameterValues = parameters.Select(parameter =>
                                                    {
                                                        if (parameter.ParameterType.IsGenericType &&
                                                            parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                                            return serviceProvider.AsNotNull()
                                                                                  .GetService(parameter.ParameterType);

                                                        return serviceProvider.AsNotNull()
                                                                              .GetRequiredService(parameter.ParameterType);
                                                    })
                                                    .ToArray();

                    var instance = (TServiceType) constructor.Invoke(parameterValues);

                    return instance;
                });
            });
        }

        [NotNull]
        private static IEnumerable<ImplementationsServiceDescriptors> GetImplementationServices(
            [NotNull] Type instanceType,
            [NotNull] [ItemNotNull] Type[] ignoredTypes)
        {
            return instanceType.GetAllInterfaces()
                               .Concat(instanceType.GetAllBaseTypes())
                               .Distinct()
                               .Except(AlwaysIgnoredTypes)
                               .Except(ignoredTypes)
                               .Where(x => x != null)
                               .Select(serviceType =>
                               {
                                   var serviceDescriptor = ServiceDescriptor.Transient(serviceType,
                                                                                       serviceProvider => serviceProvider.AsNotNull()
                                                                                           .GetRequiredService(instanceType))
                                                                            .AsNotNull();

                                   var factoryServiceDescriptor = CreateFactoryServiceDescriptor(instanceType, serviceType);

                                   var registerManyTimes = serviceType.GetCustomAttributes<RegisterManyTimesAttribute>(false)
                                                                      .Any();

                                   return new ImplementationsServiceDescriptors(serviceDescriptor, factoryServiceDescriptor, registerManyTimes);
                               })
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

            var factoryServiceDescriptor = CreateFactoryServiceDescriptor(type, type);

            serviceCollection.AddOrReplace(serviceDescriptor.AsNotNull());

            serviceCollection.AddOrReplace(factoryServiceDescriptor);
        }

        private static bool ValidateType([NotNull] Type type, [NotNull] Type[] ignoredTypes, [NotNull] [ItemNotNull] Type[] ignoredBaseTypes)
        {
            if (!type.IsClass)
                return false;

            if (type.IsAbstract)
                return false;

            if (type.IsGenericTypeDefinition)
                return false;

            if (!type.GetConstructors()
                     .Any())
                return false;

            if (ignoredTypes.Any(x => x == type))
                return false;

            if (ignoredBaseTypes.Any(type.InheritsOrImplements))
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

        private readonly struct ImplementationsServiceDescriptors
        {
            [NotNull]
            public readonly ServiceDescriptor ServiceDescriptor;

            [NotNull]
            public readonly ServiceDescriptor FactoryServiceDescriptor;

            public ImplementationsServiceDescriptors(
                [NotNull] ServiceDescriptor serviceDescriptor,
                [NotNull] ServiceDescriptor factoryServiceDescriptor,
                bool registerManyTimes)
            {
                ServiceDescriptor        = serviceDescriptor;
                FactoryServiceDescriptor = factoryServiceDescriptor;
                RegisterManyTimes        = registerManyTimes;
            }

            public readonly bool RegisterManyTimes;
        }
    }
}
