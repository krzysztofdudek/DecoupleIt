using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing.Exceptions;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Tracer implements concept of tracing.
    ///     Class is not inheritable.
    /// </summary>
    [PublicAPI]
    public sealed class Tracer : ISpan
    {
        /// <summary>
        ///     Generator of new <see cref="TracingId" />.
        /// </summary>
        [NotNull]
        public static Func<TracingId> NewTracingIdGenerator = () => (TracingId) Guid.NewGuid();

        /// <summary>
        ///     Gets span for current async flow.
        /// </summary>
        /// <exception cref="NotInTheContextOfSpan">Current thread is not in the context of any span.</exception>
        public static Span CurrentSpan => CurrentTracer._span;

        /// <summary>
        ///     Gets tracer for current async flow.
        /// </summary>
        /// <exception cref="NotInTheContextOfSpan">Current thread is not in the context of any span..</exception>
        [NotNull]
        internal static Tracer CurrentTracer =>
            Trace.Count > 0
                ? Trace.Peek()
                       .AsNotNull()
                : throw new NotInTheContextOfSpan();

        /// <summary>
        ///     Indicates if there is root span opened.
        /// </summary>
        public static bool IsRootSpanOpened => Trace.Count > 0;

        /// <summary>
        ///     Event is invoked when metric is pushed.
        /// </summary>
        [CanBeNull]
        public static event MetricPushedDelegate MetricPushed;

        /// <summary>
        ///     Event is invoked when any span is closed.
        /// </summary>
        [CanBeNull]
        public static event SpanClosedDelegate SpanClosed;

        /// <summary>
        ///     Event is invoked when a new span is opened.
        /// </summary>
        [CanBeNull]
        public static event SpanOpenedDelegate SpanOpened;

        /// <inheritdoc cref="ISpan.AttachResource" />
        [NotNull]
        public static ISpan AttachResource([NotNull] IDisposable resource, [CanBeNull] object key = default)
        {
            ((ISpan) CurrentTracer).AttachResource(resource);

            return CurrentTracer;
        }

        /// <summary>
        ///     Clears storage for current thread. It is recommended to use this method at the end of thread that used tracer. It
        ///     will provide an avoidance for potential memory leaks caused by missing disposals of spans.
        /// </summary>
        public static void Clear()
        {
            if (TraceStorage.Value == null)
                return;

            foreach (var tracer in TraceStorage.Value.ToNotNullList())
                tracer.Close();

            TraceStorage.Value = null;
        }

        /// <summary>
        ///     Gets the resource of given type and key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="TResource">Type of the resource.</typeparam>
        /// <returns>Resource.</returns>
        /// <exception cref="ResourceNotFound">Resource not found.</exception>
        [NotNull]
        public static TResource GetResource<TResource>([CanBeNull] object key = default)
            where TResource : class
        {
            foreach (var tracer in Trace.ToList())
            {
                var attachedResource = tracer.AsNotNull()
                                             ._attachedResources.FirstOrDefault(x => x.Resource is TResource && (x.Key?.Equals(key) ?? true));

                if (attachedResource != null)
                    return (TResource) attachedResource.Resource;
            }

            throw new ResourceNotFound();
        }

        /// <summary>
        ///     As tracer has to be initialized, the best way to do this is execute initialization at the beginning of thread,
        ///     where tracer will be used.
        /// </summary>
        public static void Initialize()
        {
            TraceStorage.Value = new Stack<Tracer>();
        }

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
        public static ISpan OpenChildSpan([NotNull] string name, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            if (Trace.Count == 0)
                throw new RootSpanIsNotOpened();

            var span = new Span(CurrentSpan.TraceId,
                                NewTracingIdGenerator(),
                                name,
                                CurrentSpan.Id,
                                type);

            var tracer = new Tracer(span);

            Trace.Push(tracer);

            InvokeSpanOpened(span);

            return tracer;
        }

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
        public static ISpan OpenChildSpan([NotNull] Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenChildSpan(creatorType.FullName.AsNotNull(), type);
        }

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
        public static ISpan OpenRootSpan(
            TracingId traceId,
            TracingId id,
            [NotNull] string name,
            [CanBeNull] TracingId? parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            if (Trace.Count > 0)
                throw new RootSpanIsAlreadyOpened();

            var span = new Span(traceId,
                                id,
                                name,
                                parentId,
                                type);

            var tracer = new Tracer(span);

            Trace.Push(tracer);

            InvokeSpanOpened(span);

            return tracer;
        }

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
        public static ISpan OpenRootSpan(
            TracingId traceId,
            TracingId id,
            [NotNull] Type creatorType,
            TracingId parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenRootSpan(traceId,
                                id,
                                creatorType.FullName.AsNotNull(),
                                parentId,
                                type);
        }

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
        public static ISpan OpenRootSpan([NotNull] string name, SpanType type)
        {
            var newSpanIdentifier = NewTracingIdGenerator();

            return OpenRootSpan(newSpanIdentifier,
                                newSpanIdentifier,
                                name,
                                null,
                                type);
        }

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
        public static ISpan OpenRootSpan([NotNull] Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenRootSpan(creatorType.FullName.AsNotNull(), type);
        }

        /// <inheritdoc cref="ISpan.PushMetric" />
        public static void PushMetric(Metric metric)
        {
            ((ISpan) CurrentTracer).PushMetric(metric);
        }

        /// <inheritdoc />
        public TimeSpan Duration => _stopwatch.Elapsed;

        /// <inheritdoc />
        public IReadOnlyCollection<Metric> Metrics => _metrics;

        [NotNull]
        private static readonly AsyncLocal<Stack<Tracer>> TraceStorage = new AsyncLocal<Stack<Tracer>>();

        /// <summary>
        ///     Contains stack of spans for current async flow.
        /// </summary>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        [NotNull]
        [ItemNotNull]
        private static Stack<Tracer> Trace => TraceStorage.Value ?? throw new TraceIsNotInitialized();

        private static void InvokeSpanClosed(Span span, TimeSpan duration)
        {
            SpanClosed?.Invoke(span, duration);
        }

        private static void InvokeSpanOpened(Span span)
        {
            SpanOpened?.Invoke(span);
        }

        [NotNull]
        [ItemNotNull]
        private readonly List<AttachedResource> _attachedResources = new List<AttachedResource>();

        private bool _isDisposed;

        [NotNull]
        private readonly List<Metric> _metrics = new List<Metric>();

        private readonly Span _span;

        [NotNull]
        private readonly Stopwatch _stopwatch;

        private event MetricPushedDelegate InstanceMetricPushed;

        event MetricPushedDelegate ISpan.MetricPushed
        {
            add => InstanceMetricPushed += value;
            remove => InstanceMetricPushed -= value;
        }

        private Tracer(Span span)
        {
            _span      = span;
            _stopwatch = Stopwatch.StartNew();
        }

        ISpan ISpan.AttachResource(object resource, object key)
        {
            ContractGuard.IfArgumentIsNull(nameof(resource), resource);

            CheckIfDisposed();

            _attachedResources.Add(new AttachedResource(resource, key));

            return this;
        }

        private void CheckIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("Tracer has been disposed.");
        }

        private void Close()
        {
            CheckIfDisposed();

            if (CurrentTracer != this)
                throw new InvalidOperationException("Can not dispose if it's not current span.");

            Trace.Pop();

            foreach (var attachedResource in _attachedResources.ToNotNullList())
            {
                if (attachedResource.Resource is IDisposable disposable)
                    disposable.Dispose();

                _attachedResources.Remove(attachedResource);
            }

            _stopwatch.Stop();

            _isDisposed = true;

            InvokeSpanClosed(_span, Duration);
        }

        void IDisposable.Dispose()
        {
            if (_isDisposed)
                return;

            Close();
        }

        private void InvokeMetricPushed(Metric metric)
        {
            MetricPushed?.Invoke(_span, metric);
            InstanceMetricPushed?.Invoke(_span, metric);
        }

        void ISpan.PushMetric(Metric metric)
        {
            _metrics.Add(metric);

            InvokeMetricPushed(metric);
        }

        private sealed class AttachedResource
        {
            public readonly object Key;

            [NotNull]
            public readonly object Resource;

            public AttachedResource([NotNull] object resource, object key)
            {
                Resource = resource;
                Key      = key;
            }
        }
    }
}
