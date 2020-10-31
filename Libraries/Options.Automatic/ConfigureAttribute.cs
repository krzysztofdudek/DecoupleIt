using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks an options class to be configured automatically. Class can not be inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public sealed class ConfigureAttribute : Attribute, IConfigureAttribute
    {
        /// <summary>
        ///     Configuration path.
        /// </summary>
        [CanBeNull]
        public string ConfigurationPath { get; }

        /// <inheritdoc />
        public short Priority { get; set; }

        /// <summary>
        ///     Marks an options class to be configured automatically from configuration path specified in <paramref name="configurationPath" />.
        /// </summary>
        /// <param name="configurationPath">Configuration path.</param>
        public ConfigureAttribute([NotNull] string configurationPath)
        {
            ConfigurationPath = configurationPath;
        }

        /// <summary>
        ///     Marks an options class to be configured automatically from [namespace].[class name without "Options"] configuration path.
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
        ///         , leads to loading configuration from path "SomeNamespace:App:Example".
        ///     </para>
        /// </remarks>
        public ConfigureAttribute() { }
    }
}
