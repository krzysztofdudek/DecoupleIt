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
        ///     Parent tracer span.
        /// </summary>
        [CanBeNull]
        ITracerSpan Parent { get; }
    }
}
