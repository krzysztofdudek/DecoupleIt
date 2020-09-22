using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks options class to be configured automatically with the same path as namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [PublicAPI]
    public sealed class ConfigureAsNamespaceAttribute : Attribute { }
}
