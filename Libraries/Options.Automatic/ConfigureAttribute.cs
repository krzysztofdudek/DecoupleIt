using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     Marks options class to be configured automatically.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    [PublicAPI]
    public sealed class ConfigureAttribute : Attribute
    {
        /// <summary>
        ///     Configuration section name.
        /// </summary>
        [CanBeNull]
        public string ConfigurationSectionName { get; }

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
