using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Options of a service.
    /// </summary>
    [PublicAPI]
    [ConfigureAsNamespace]
    public class Options
    {
        /// <summary>
        ///     Logging options.
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
            ///     If request should be logged.
            /// </summary>
            public bool LogRequests { get; set; } = true;

            /// <summary>
            ///     Should request bodies be logged.
            /// </summary>
            public bool LogRequestsBodies { get; set; } = true;

            /// <summary>
            ///     If responses should be logged.
            /// </summary>
            public bool LogResponses { get; set; } = true;

            /// <summary>
            ///     Should response bodies be logged.
            /// </summary>
            public bool LogResponsesBodies { get; set; } = true;
        }
    }
}
