using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Tracing options.
    /// </summary>
    [PublicAPI]
    [Configure]
    public sealed class TracingOptions
    {
        /// <summary>
        ///     Headers.
        /// </summary>
        [NotNull]
        public HeadersOptions Headers { get; set; } = new HeadersOptions();

        /// <summary>
        ///     Http headers options of tracing.
        /// </summary>
        [PublicAPI]
        public sealed class HeadersOptions
        {
            /// <summary>
            ///     Parent span id header name. Default: "X-B3-ParentSpanId".
            /// </summary>
            [NotNull]
            public string ParentSpanIdHeaderName { get; set; } = "X-B3-ParentSpanId";

            /// <summary>
            ///     Span id header name. Default: "X-B3-SpanId".
            /// </summary>
            [NotNull]
            public string SpanIdHeaderName { get; set; } = "X-B3-SpanId";

            /// <summary>
            ///     Span name header name. Default: "X-SpanName".
            /// </summary>
            [NotNull]
            public string SpanNameHeaderName { get; set; } = "X-SpanName";

            /// <summary>
            ///     Trace id header name. Default: "X-B3-TraceId".
            /// </summary>
            [NotNull]
            public string TraceIdHeaderName { get; set; } = "X-B3-TraceId";
        }
    }
}
