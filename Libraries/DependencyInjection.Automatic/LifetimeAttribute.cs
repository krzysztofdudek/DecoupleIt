using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.DependencyInjection.Automatic
{
    /// <summary>
    ///     Base class for lifetime attribute.
    /// </summary>
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [PublicAPI]
    public abstract class LifetimeAttribute : Attribute
    {
        /// <summary>
        ///     Defines in which environments implementation should be registered. Split environment names with ';'.
        /// </summary>
        public string Environments { get; set; }
    }
}
