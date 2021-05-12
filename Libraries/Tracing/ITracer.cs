using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Tracer implements concept of tracing.
    ///     Class is not inheritable.
    /// </summary>
    [PublicAPI]
    public interface ITracer
    {
        /// <summary>
        ///     Gets span for current async flow.
        /// </summary>
        [CanBeNull]
        ITracerSpan CurrentSpan { get; }

        /// <summary>
        ///     Event is invoked when any span is closed.
        /// </summary>
        [CanBeNull]
        event SpanClosedDelegate SpanClosed;

        /// <summary>
        ///     Event is invoked when a new span is opened.
        /// </summary>
        [CanBeNull]
        event SpanOpenedDelegate SpanOpened;

        /// <summary>
        ///     Opens child span.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        [MustUseReturnValue]
        [NotNull]
        ITracerSpan OpenSpan([NotNull] string name, SpanType type);

        /// <summary>
        ///     Opens child span.
        /// </summary>
        /// <param name="creatorType">Type of span creator.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        [MustUseReturnValue]
        [NotNull]
        ITracerSpan OpenSpan([NotNull] Type creatorType, SpanType type);

        /// <summary>
        ///     Opens root span.
        /// </summary>
        /// <param name="traceId">Trace id.</param>
        /// <param name="id">Id.</param>
        /// <param name="name">Name.</param>
        /// <param name="parentId">Parent span id.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        [MustUseReturnValue]
        [NotNull]
        ITracerSpan OpenSpan(
            TracingId traceId,
            TracingId id,
            [NotNull] string name,
            [CanBeNull] TracingId? parentId,
            SpanType type);

        /// <summary>
        ///     Opens root span.
        /// </summary>
        /// <param name="traceId">Trace id.</param>
        /// <param name="id">Id.</param>
        /// <param name="creatorType">Type of span creator.</param>
        /// <param name="parentId">Parent span id.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        [MustUseReturnValue]
        [NotNull]
        ITracerSpan OpenSpan(
            TracingId traceId,
            TracingId id,
            [NotNull] Type creatorType,
            TracingId? parentId,
            SpanType type);
    }
}
