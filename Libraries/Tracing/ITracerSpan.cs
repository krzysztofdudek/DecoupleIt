using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Represents span.
    /// </summary>
    [PublicAPI]
    public interface ITracerSpan : IDisposable
    {
        /// <summary>
        ///     Description of span.
        /// </summary>
        SpanDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the duration of scope.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     Attached disposable resource to this instance. It will be disposed on scope disposal.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <exception cref="ObjectDisposedException">Tracer has been disposed.</exception>
        void AttachResource([NotNull] IDisposable resource);
    }
}
