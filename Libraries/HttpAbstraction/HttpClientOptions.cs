using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Options of http clients.
    /// </summary>
    [PublicAPI]
    public sealed class HttpClientOptions
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
        ///     Skip SSL certificate validation.
        /// </summary>
        public bool SkipSSLCertificateValidation { get; set; }

        /// <summary>
        ///     Timeout in milliseconds. Default: 10000ms.
        /// </summary>
        public int TimeoutMs { get; set; } = 10000;
    }
}
