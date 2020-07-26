using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks class as registrable with singleton lifetime be
    ///     <see cref="ServiceCollectionExtensions.ScanAssemblyForImplementations" />. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class SingletonAttribute : LifeTimeAttribute { }
}
