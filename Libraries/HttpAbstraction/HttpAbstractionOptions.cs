using System;
using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Options of http clients.
    /// </summary>
    [ConfigureAsNamespace]
    [PublicAPI]
    public sealed class HttpAbstractionOptions
    {
        /// <summary>
        ///     Host's identifier.
        /// </summary>
        public Guid HostIdentifier { get; set; }

        /// <summary>
        ///     Host's identifier header name. Default: "X-HostIdentifier".
        /// </summary>
        [NotNull]
        public string HostIdentifierHeaderName { get; set; } = "X-HostIdentifier";

        /// <summary>
        ///     Host's name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        ///     Host's name header name. Default: "X-HostName".
        /// </summary>
        [NotNull]
        public string HostNameHeaderName { get; set; } = "X-HostName";

        /// <summary>
        ///     Host's version.
        /// </summary>
        public string HostVersion { get; set; }

        /// <summary>
        ///     Host's version header name. Default: "X-HostVersion".
        /// </summary>
        [NotNull]
        public string HostVersionHeaderName { get; set; } = "X-HostVersion";

        /// <summary>
        ///     Parent span id header name. Default: "X-B3-ParentSpanId".
        /// </summary>
        [NotNull]
        public string ParentSpanIdHeaderName { get; set; } = "X-B3-ParentSpanId";

        /// <summary>
        ///     Skip SSL certificate validation.
        /// </summary>
        public bool SkipSSLCertificateValidation { get; set; }

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
        ///     Timeout in milliseconds. Default: 10000ms.
        /// </summary>
        public int TimeoutMs { get; set; } = 10000;

        /// <summary>
        ///     Trace id header name. Default: "X-B3-TraceId".
        /// </summary>
        [NotNull]
        public string TraceIdHeaderName { get; set; } = "X-B3-TraceId";
    }
}
