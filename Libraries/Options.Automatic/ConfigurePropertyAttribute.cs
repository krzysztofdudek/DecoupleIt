using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks property of a options class to be configured automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public sealed class ConfigurePropertyAttribute : Attribute, IConfigureAttribute
    {
        /// <summary>
        ///     Should null value
        /// </summary>
        public bool AssignNull { get; set; }

        /// <summary>
        ///     Configuration path.
        /// </summary>
        [CanBeNull]
        public string ConfigurationPath { get; }

        /// <inheritdoc />
        public short Priority { get; set; }

        /// <summary>
        ///     Marks property of a options class to be configured automatically from configuration path specified in <paramref name="configurationPath" />.
        /// </summary>
        /// <param name="configurationPath">Configuration path.</param>
        public ConfigurePropertyAttribute([NotNull] string configurationPath)
        {
            ConfigurationPath = configurationPath;
        }
    }
}
