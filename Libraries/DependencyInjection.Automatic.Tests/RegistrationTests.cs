using System;
using System.Linq;
using DifferentNamespace;
using GS.DecoupleIt.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable All

namespace GS.DecoupleIt.DependencyInjection.Automatic.Tests
{
    public class RegistrationTests
    {
        public RegistrationTests()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.ScanAssemblyForImplementations(typeof(RegistrationTests).Assembly,
                                                             "GS.DecoupleIt.DependencyInjection.Automatic.Tests",
                                                             ignoredTypes: new[]
                                                             {
                                                                 typeof(IgnoredClass)
                                                             },
                                                             ignoredBaseTypes: new[]
                                                             {
                                                                 typeof(IgnoredBaseClass)
                                                             },
                                                             registerAsManyTypes: new[]
                                                             {
                                                                 typeof(ConfigurableRegisterAsManyClass)
                                                             });

            _serviceProvider = serviceCollection.BuildServiceProvider()
                                                .AsNotNull();
        }

        [JetBrains.Annotations.NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        private readonly IServiceProvider _serviceProvider;

        [Fact]
        public void CannotResolveAbstractClass()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<AbstractClass>());
        }

        [Fact]
        public void CannotResolveClassFromDifferentNamespaceThatRegistrationWasCalledFor()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<ClassFromDifferentNamespace>());
        }

        [Fact]
        public void CannotResolveClassNotMarkedWithLifetimeAttribute()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<NotRegisteredObject>());
        }

        [Fact]
        public void CannotResolveClassThatIsIgnored()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<IgnoredClass>());
        }

        [Fact]
        public void CannotResolveIgnoredBaseTypeNorInheritors()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<IgnoredBaseClass>());
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<IgnoredDerivedClass>());
        }

        [Fact]
        public void CannotResolveInterfaceNotMarkedWithLifetimeAttribute()
        {
            Assert.Throws<InvalidOperationException>(() => _serviceProvider.GetRequiredService<INotRegisteredInterface>());
        }

        [Fact]
        public void CanReoslveClassThatInheritsFromIgnoredClassBecauseOfAttribute()
        {
            _serviceProvider.GetRequiredService<NotIgnoredClass>();
        }

        [Fact]
        public void CanResolveAllImplementationsOfInterfaceMarkedWithRegisterManyTimes()
        {
            var instances = _serviceProvider.GetServices<IBaseInterface>()
                                            .ToList();

            Assert.Equal(3, instances.Count);

            Assert.True(1 == instances.OfType<SingletonObject>()
                                      .Count());

            Assert.True(1 == instances.OfType<TransientObject>()
                                      .Count());

            Assert.True(1 == instances.OfType<ScopedObject>()
                                      .Count());
        }

        [Fact]
        public void CanResolveAllImplementationsOfTypeMarkedAsRegisterAsManyByConfiguration()
        {
            Assert.Equal(2,
                         _serviceProvider.GetServices<ConfigurableRegisterAsManyClass>()
                                         .Count());
        }

        [Fact]
        public void CanResolveClassRegisteredAsSelf()
        {
            _serviceProvider.GetRequiredService<ClassRegisteredAsSelf>();
        }

        [Fact]
        public void CanResolveClassRegisteredByInterface()
        {
            _serviceProvider.GetRequiredService<ClassRegisteredByInterface>();
        }

        [Fact]
        public void CanResolveClassThatInheritsClassRegisteredAsSelf()
        {
            _serviceProvider.GetRequiredService<ClassNotRegisteredByBaseClass>();
        }

        [Fact]
        public void CanResolveGenericInterfaceIfItHasImplementation()
        {
            _serviceProvider.GetRequiredService<IRepository<TransientObject>>();
        }

        [Fact]
        public void CanResolveScoped()
        {
            ScopedObject instance1, instance2, instance3, instance4;

            using (var scope = _serviceProvider.CreateScope())
            {
                instance1 = scope.ServiceProvider.GetService<ScopedObject>();
                instance2 = scope.ServiceProvider.GetService<ScopedObject>();
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                instance3 = scope.ServiceProvider.GetService<ScopedObject>();
                instance4 = scope.ServiceProvider.GetService<ScopedObject>();
            }

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
            Assert.Equal(instance3.GetHashCode(), instance4.GetHashCode());
            Assert.NotEqual(instance1.GetHashCode(), instance3.GetHashCode());
        }

        [Fact]
        public void CanResolveSingleton()
        {
            var instance1 = _serviceProvider.GetService<SingletonObject>();
            var instance2 = _serviceProvider.GetService<SingletonObject>();

            Assert.Equal(instance1.GetHashCode(), instance2.GetHashCode());
        }

        [Fact]
        public void CanResolveTransient()
        {
            var instance1 = _serviceProvider.GetService<TransientObject>();
            var instance2 = _serviceProvider.GetService<TransientObject>();

            Assert.NotEqual(instance1.GetHashCode(), instance2.GetHashCode());
        }

        [Fact]
        public void ClassRegisteredLaterOverridesPreviousImplementation()
        {
            var instanceOfInheritor = _serviceProvider.GetRequiredService<ClassNotRegisteredByBaseClass>();
            var instanceOfBaseClass = _serviceProvider.GetRequiredService<ClassRegisteredAsSelf>();

            Assert.Equal(instanceOfInheritor.GetHashCode(), instanceOfBaseClass.GetHashCode());
        }

        [Fact]
        public void SingletonResolvedByImplementationClassOrInterfaceGivesTheSameInstance()
        {
            var implementationInstance = _serviceProvider.GetRequiredService<SingletonObject>();
            var interfaceInstance      = _serviceProvider.GetRequiredService<ISingletonInterface>();

            Assert.Equal(implementationInstance.GetHashCode(), interfaceInstance.GetHashCode());
        }
    }
}
