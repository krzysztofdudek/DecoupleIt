using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks options class to be configured automatically.
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
        ///     Marks options class to be configured automatically from configuration path specified in <paramref name="configurationPath" />.
        /// </summary>
        /// <param name="configurationPath">Configuration path.</param>
        public ConfigureAttribute([NotNull] string configurationPath)
        {
            ConfigurationPath = configurationPath;
        }

        /// <summary>
        ///     Marks options class to be configured automatically from:<br />
        ///     [namespace].[class name without "Options"] configuration path.<br />
        /// </summary>
        public ConfigureAttribute() { }
    }
}
