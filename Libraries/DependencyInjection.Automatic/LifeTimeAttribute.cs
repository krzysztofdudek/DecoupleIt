using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Base class for lifetime attribute.
    /// </summary>
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [PublicAPI]
    public abstract class LifeTimeAttribute : Attribute
    {
        /// <summary>
        ///     Defines in which environments implementation should be registered.
        /// </summary>
        public string Environments { get; set; }
    }
}
