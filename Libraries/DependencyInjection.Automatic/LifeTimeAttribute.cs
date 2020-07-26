using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Base class for lifetime attributes.
    /// </summary>
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public abstract class LifeTimeAttribute : Attribute { }
}
