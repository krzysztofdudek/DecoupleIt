using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling
{
    /// <summary>
    ///     Represents configuration of scheduling module.
    /// </summary>
    [PublicAPI]
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Logging options.
        /// </summary>
        [NotNull]
        public LoggingOptions Logging { get; set; } = new();

        [PublicAPI]
        public class LoggingOptions
        {
            /// <summary>
            ///     Enables logging of additional information about flows that are executed. Error level logs are always enabled despite of this setting.
            /// </summary>
            public bool EnableNonErrorLogging { get; set; }
        }
    }
}
