using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Services dns options.
    /// </summary>
    [PublicAPI]
    public sealed class ServicesUrisOptions : Dictionary<string, string>
    {
        /// <summary>
        ///     Configuration section name.
        /// </summary>
        [NotNull]
        public const string ConfigurationSectionName = "ServicesUris";
    }
}
