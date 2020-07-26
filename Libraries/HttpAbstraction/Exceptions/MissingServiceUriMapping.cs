using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.HttpAbstraction.Exceptions
{
    /// <summary>
    ///     Exception thrown when http client tries to use service that is not mapped to url.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    public sealed class MissingServiceUriMapping : Exception
    {
        /// <inheritdoc />
        [NotNull]
        public override string Message => $"Service named \"{ServiceName}\" misses uri mapping.";

        /// <summary>
        ///     Service name.
        /// </summary>
        [NotNull]
        [PublicAPI]
        public string ServiceName { get; }

        /// <summary>
        ///     Creates an instance of <see cref="MissingServiceUriMapping" />.
        /// </summary>
        /// <param name="serviceName">Service name.</param>
        public MissingServiceUriMapping([NotNull] string serviceName)
        {
            ServiceName = serviceName;
        }
    }
}
