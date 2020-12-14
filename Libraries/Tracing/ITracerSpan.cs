using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Interface represents tracer span.
    /// </summary>
    public interface ITracerSpan : IDisposable
    {
        /// <summary>
        ///     Description of the span.
        /// </summary>
        SpanDescriptor Descriptor { get; }

        /// <summary>
        ///     Gets the duration of the span.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        ///     Attached disposable resource to this instance. It will be disposed on span disposal.
        /// </summary>
        /// <param name="resource">A resource.</param>
        /// <exception cref="ObjectDisposedException">The tracer has been disposed.</exception>
        void AttachResource([NotNull] IDisposable resource);
    }
}
