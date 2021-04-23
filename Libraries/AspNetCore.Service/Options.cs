using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;
using Microsoft.IO;

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
            ///     Enables logging. It's enabled by default.
            /// </summary>
            public bool Enabled { get; set; } = true;

            /// <summary>
            ///     Enables logging of requests. It's enabled by default.
            /// </summary>
            public bool LogRequests { get; set; } = true;

            /// <summary>
            ///     Enables logging of requests' bodies. It's enabled by default.
            /// </summary>
            public bool LogRequestsBodies { get; set; } = true;

            /// <summary>
            ///     Enables logging of requests' headers. It's enabled by default.
            /// </summary>
            public bool LogRequestsHeaders { get; set; } = true;

            /// <summary>
            ///     Enables logging of responses. It's enabled by default.
            /// </summary>
            public bool LogResponses { get; set; } = true;

            /// <summary>
            ///     Enables logging of responses' bodies. It's enabled by default.
            /// </summary>
            public bool LogResponsesBodies { get; set; } = true;

            /// <summary>
            ///     Enables logging of responses' headers. It's enabled by default.
            /// </summary>
            public bool LogResponsesHeaders { get; set; } = true;

            /// <summary>
            ///     Middleware options.
            /// </summary>
            public MiddlewareOptions Middleware { get; set; } = new();
        }

        /// <summary>
        ///     Middleware options.
        /// </summary>
        [PublicAPI]
        public sealed class MiddlewareOptions
        {
            /// <summary>
            ///     Property configuring <see cref="RecyclableMemoryStreamManager" />. Each large buffer will be a multiple of this value.
            /// </summary>
            public int LargeBufferBlockSizeMultiple { get; set; } = 1024 * 1024;

            /// <summary>
            ///     Property configuring <see cref="RecyclableMemoryStreamManager" />. How many bytes of large free buffers to allow before we start dropping those returned to
            ///     us.
            /// </summary>
            public int LargeBufferMaximumPoolBytes { get; set; } = 1024 * 1024 * 16 * 4;

            /// <summary>
            ///     Property configuring <see cref="RecyclableMemoryStreamManager" />. Buffers larger than this are not pooled.
            /// </summary>
            public int MaximumSingleBufferSize { get; set; } = 1024 * 1024 * 16;

            /// <summary>
            ///     Property configuring <see cref="RecyclableMemoryStreamManager" />. Size of each block that is pooled. Must be &gt; 0.
            /// </summary>
            public int SmallBufferBlockSize { get; set; } = 1024 * 16;

            /// <summary>
            ///     Property configuring <see cref="RecyclableMemoryStreamManager" />. How many bytes of small free blocks to allow before we start dropping those returned to
            ///     us.
            /// </summary>
            public int SmallBufferMaximumPoolBytes { get; set; } = 1024 * 16 * 1024;
        }
    }
}
