using System;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Marks interface or class as registrable for many times instead of overriding preceding registration of component.
    ///     It's used by <see cref="ServiceCollectionExtensions.ScanAssemblyForImplementations" />. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class RegisterManyTimesAttribute : Attribute { }
}
