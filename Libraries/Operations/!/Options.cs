using System.Collections.Generic;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Options of operations.
    /// </summary>
    [PublicAPI]
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Disables creating scopes by commands. It can be used for testing purposes, to access internal events emitted by tested operation. It's disabled by default.
        /// </summary>
        public bool CommandDoNotCreateOwnScope { get; set; }

        /// <summary>
        ///     Logging.
        /// </summary>
        [NotNull]
        public LoggingOptions Logging { get; set; } = new();

        /// <summary>
        ///     Logging options.
        /// </summary>
        [PublicAPI]
        public sealed class LoggingOptions
        {
            /// <summary>
            ///     Map that allows to remap Critical logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> CriticalRemap { get; set; } = new();

            /// <summary>
            ///     Map that allows to remap Debug logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> DebugRemap { get; set; } = new();

            /// <summary>
            ///     Enables logging of additional information about flows that are executed. Error level logs are always enabled despite of this setting.
            /// </summary>
            public bool EnableNonErrorLogging { get; set; }

            /// <summary>
            ///     Map that allows to remap Error logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> ErrorRemap { get; set; } = new();

            /// <summary>
            ///     Map that allows to remap log level for specific exception categories.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> ExceptionCategoryRemap { get; set; } = new();

            /// <summary>
            ///     Map that allows to remap Information logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> InformationRemap { get; set; } = new();

            /// <summary>
            ///     Map that allows to remap Trace logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> TraceRemap { get; set; } = new();

            /// <summary>
            ///     Map that allows to remap Warning logs to another level for specific classes.
            /// </summary>
            [NotNull]
            public Dictionary<string, LogLevel> WarningRemap { get; set; } = new();
        }
    }
}
