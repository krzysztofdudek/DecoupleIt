using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks an options class to be configured automatically with the configuration path equal to it's namespace. Class can not be inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public sealed class ConfigureAsNamespaceAttribute : Attribute, IConfigureAttribute
    {
        /// <inheritdoc />
        public short Priority { get; set; }

        /// <summary>
        ///     Marks an options class to be configured automatically with the configuration path equal to it's namespace.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Example:
        ///         <code>
        ///             namespace SomeNamespace.App
        ///             {
        ///                 [Configure]
        ///                 public class ExampleOptions
        ///                 {
        ///                     public string Property { get; set; }
        ///                 }
        ///             }
        ///         </code>
        ///         , leads to loading configuration from path "SomeNamespace:App".
        ///     </para>
        /// </remarks>
        [UsedImplicitly]
        public ConfigureAsNamespaceAttribute() { }
    }
}
