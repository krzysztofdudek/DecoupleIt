// ReSharper disable All

namespace GS.DecoupleIt.DependencyInjection.Automatic.Tests
{
    [RegisterManyTimes]
    public interface IBaseInterface { }

    public abstract class BaseClass : IBaseInterface { }

    [Singleton]
    public abstract class AbstractClass : IBaseInterface { }

    public interface ISingletonInterface { }

    [Singleton]
    public class SingletonObject : BaseClass, ISingletonInterface { }

    [Transient]
    public class TransientObject : BaseClass { }

    [Scoped]
    public class ScopedObject : BaseClass { }

    public class NotRegisteredObject : BaseClass { }

    public interface INotRegisteredInterface { }

    [Singleton]
    public interface IInterfaceMeaningRegistration { }

    public class ClassRegisteredByInterface : IInterfaceMeaningRegistration { }

    [Singleton]
    public class ClassRegisteredAsSelf { }

    public class ClassNotRegisteredByBaseClass : ClassRegisteredAsSelf { }

    [Singleton]
    public interface IRepository<TEntity> { }

    public class RepositoryOfTransientObjects : IRepository<TransientObject> { }

    [Singleton]
    public class IgnoredClass { }

    public class NotIgnoredClass : IgnoredClass { }

    [Singleton]
    public class IgnoredBaseClass { }

    public class IgnoredDerivedClass : IgnoredBaseClass { }

    [Singleton]
    public abstract class ConfigurableRegisterAsManyClass { }

    public class ConfigurableRegisterAsManyClassInheritor1 : ConfigurableRegisterAsManyClass { }

    public class ConfigurableRegisterAsManyClassInheritor2 : ConfigurableRegisterAsManyClass { }
}
