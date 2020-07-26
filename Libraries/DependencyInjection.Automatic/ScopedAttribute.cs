using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks class as registrable with scoped lifetime be
    ///     <see cref="ServiceCollectionExtensions.ScanAssemblyForImplementations" />. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class ScopedAttribute : LifeTimeAttribute { }
}
