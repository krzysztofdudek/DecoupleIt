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
    ///     Extends <see cref="IServiceCollection" /> with methods automatically registering dependencies in it.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Method automatically registers all found services in the assembly if they're annotated with one of this three attributes: <br />
        ///         - <see cref="SingletonAttribute" /><br />
        ///         - <see cref="TransientAttribute" /><br />
        ///         - <see cref="ScopedAttribute" /><br />
        ///     </para>
        ///     By marking a class with one of those attributes, we say that this class should be registered with all its base classes and implemented interfaces as
        ///     service.
        ///     By marking base class or interface with one of those attributes, we say that all implementors or derived classes will be registered implicitly.
        ///     By default all services are registered only one time, which means that all implementations of a specific service will override the previous one registered.
        ///     To make a service be registered many times if we need more than one implementation of it, the service can be annotated with
        ///     <see cref="RegisterManyTimesAttribute" /> or passed to <paramref name="registerAsManyTypes" /> parameter.
        ///     Additionally this method will register a factory for any service type.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="assembly">Assembly to scan.</param>
        /// <param name="namespace">Namespace filter for registered types. Only types being included in such namespace will be registered..</param>
        /// <param name="ignoredTypes">
        ///     Types that will be ignored in the process of scanning. Implementations of specific services can be passed.
        /// </param>
        /// <param name="ignoredBaseTypes">
        ///     Types that will exclude found type if it's derived from or is implementing it.
        /// </param>
        /// <param name="registerAsManyTypes">
        ///     Services that will be registered multiple times instead of being overriden. This parameter is usable for registering interfaces as a services when we don't
        ///     have access to it's definition or we don't want to mark it with <see cref="RegisterManyTimesAttribute" />.
        /// </param>
        public static void ScanAssemblyForImplementations(
            [NotNull] [ItemNotNull] this IServiceCollection serviceCollection,
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
                ProcessSingleImplementationType(serviceCollection,
                                                ignoredTypes,
                                                registerAsManyTypes,
                                                implementationType);
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

        private static void AddOrReplace([NotNull] this IServiceCollection serviceCollection, [NotNull] ServiceDescriptor serviceDescriptor)
        {
            foreach (var existingServiceDescriptor in serviceCollection.Where(x => x.AsNotNull()
                                                                                    .ServiceType == serviceDescriptor.ServiceType)
                                                                       .ToList())
                serviceCollection.Remove(existingServiceDescriptor);

            serviceCollection.Add(serviceDescriptor);
        }

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

        private static void ProcessSingleImplementationType(
            [NotNull] [ItemNotNull] IServiceCollection serviceCollection,
            [NotNull] [ItemNotNull] Type[] ignoredTypes,
            [NotNull] [ItemNotNull] Type[] registerAsManyTypes,
            [NotNull] Type implementationType)
        {
            // Do not register the same services again.
            if (serviceCollection.Any(x => x.ServiceType == implementationType && x.ImplementationType == implementationType))
                return;

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
