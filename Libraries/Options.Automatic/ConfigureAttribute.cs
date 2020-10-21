using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks options class to be configured automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public sealed class ConfigureAttribute : Attribute, IConfigureAttribute
    {
        /// <summary>
        ///     Configuration section name.
        /// </summary>
        [CanBeNull]
        public string ConfigurationSectionName { get; }

        /// <inheritdoc />
        public short Priority { get; }

        /// <summary>
        ///     Creates an instance of <see cref="ConfigureAttribute" />. Sets custom configuration section name.
        /// </summary>
        /// <param name="configurationSectionName">Configuration section name.</param>
        public ConfigureAttribute([CanBeNull] string configurationSectionName)
        {
            ConfigurationSectionName = configurationSectionName;
        }

        /// <summary>
        ///     Creates an instance of <see cref="ConfigureAttribute" />. Uses default configuration section name, namespace of
        ///     options class.
        /// </summary>
        public ConfigureAttribute() { }
    }
}
