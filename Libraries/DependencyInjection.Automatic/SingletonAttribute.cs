using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks a service type or it's implementation to be registered singleton.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class SingletonAttribute : LifetimeAttribute { }
}
