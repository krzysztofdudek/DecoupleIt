using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks a service type or it's implementation to be registered transient.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class TransientAttribute : LifeTimeAttribute { }
}
