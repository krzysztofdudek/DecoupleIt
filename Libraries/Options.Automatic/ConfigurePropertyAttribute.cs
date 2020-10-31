using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Options.Automatic
{
    /// <summary>
    ///     <para>
    ///         Marks a property of a options class to be configured automatically. It works if options class is already annotated with:<br />
    ///         - <see cref="ConfigureAttribute" /><br />
    ///         - <see cref="ConfigureAsNamespaceAttribute" />
    ///     </para>
    ///     It can be usable when remapping old configuration path to new ones. Class is not inheritable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    [MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    [PublicAPI]
    public sealed class ConfigurePropertyAttribute : Attribute, IConfigureAttribute
    {
        /// <summary>
        ///     Should null value be assigned. It may lead to an overwriting proper value by not existing section.
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
        ///     <para>
        ///         Marks a property of a options class to be configured automatically. It works if options class is already annotated with:<br />
        ///         - <see cref="ConfigureAttribute" /><br />
        ///         - <see cref="ConfigureAsNamespaceAttribute" />
        ///     </para>
        ///     It can be usable when remapping old configuration path to new ones.
        /// </summary>
        /// <param name="configurationPath">Configuration path.</param>
        public ConfigurePropertyAttribute([NotNull] string configurationPath)
        {
            ConfigurationPath = configurationPath;
        }
    }
}
