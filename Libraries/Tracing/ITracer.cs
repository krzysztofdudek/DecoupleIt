using System;
using GS.DecoupleIt.Tracing.Exceptions;
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
        /// <exception cref="NotInTheContextOfSpan">Current thread is not in the context of any span.</exception>
        [NotNull]
        ITracerSpan CurrentSpan { get; }

        /// <summary>
        ///     Indicates if there is root span opened.
        /// </summary>
        bool IsRootSpanOpened { get; }

        /// <summary>
        ///     Generator of new <see cref="TracingId" />.
        /// </summary>
        [NotNull]
        public Func<TracingId> NewTracingIdGenerator { get; set; }

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
        ///     Clears storage for current thread. It is recommended to use this method at the end of thread that used tracer. It
        ///     will provide an avoidance for potential memory leaks caused by missing disposals of spans.
        /// </summary>
        void Clear();

        /// <summary>
        ///     As tracer has to be initialized, the best way to do this is execute initialization at the beginning of thread,
        ///     where tracer will be used.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Opens child span.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsNotOpened">Root span is not opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenChildSpan([NotNull] string name, SpanType type);

        /// <summary>
        ///     Opens child span.
        /// </summary>
        /// <param name="creatorType">Type of span creator.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsNotOpened">Root span is not opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenChildSpan([NotNull] Type creatorType, SpanType type);

        /// <summary>
        ///     Opens root span.
        /// </summary>
        /// <param name="traceId">Trace id.</param>
        /// <param name="id">Id.</param>
        /// <param name="name">Name.</param>
        /// <param name="parentId">Parent span id.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsAlreadyOpened">Root span already opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenRootSpan(
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
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsAlreadyOpened">Root span already opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenRootSpan(
            TracingId traceId,
            TracingId id,
            [NotNull] Type creatorType,
            TracingId? parentId,
            SpanType type);

        /// <summary>
        ///     Opens root span.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsAlreadyOpened">Root span already opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenRootSpan([NotNull] string name, SpanType type);

        /// <summary>
        ///     Opens root span.
        /// </summary>
        /// <param name="creatorType">Type of span creator.</param>
        /// <param name="type">Type.</param>
        /// <returns>Span lifetime.</returns>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        /// <exception cref="RootSpanIsAlreadyOpened">Root span already opened.</exception>
        [NotNull]
        [MustUseReturnValue]
        ITracerSpan OpenRootSpan([NotNull] Type creatorType, SpanType type);
    }
}
