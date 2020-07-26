using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Indicates that class should be registered as self by
    ///     <see cref="ServiceCollectionExtensions.ScanAssemblyForImplementations" />. It's usable when class does not
    ///     implement any interface. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class RegisterAsSelfAttribute : Attribute { }
}
