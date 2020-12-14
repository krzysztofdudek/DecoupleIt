using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Tracing
{
    /// <summary>
    ///     Tracer implements concept of tracing.
    ///     Class is not inheritable.
    /// </summary>
    [PublicAPI]
    [Singleton]
    internal sealed class Tracer : ITracer
    {
        /// <inheritdoc />
        public ITracerSpan CurrentSpan =>
            Trace.Count > 0
                ? Trace.Last()
                       .AsNotNull()
                : throw new NotInTheContextOfSpan();

        /// <inheritdoc />
        public bool IsRootSpanOpened => Trace.Count > 0;

        /// <inheritdoc />
        public Func<TracingId> NewTracingIdGenerator { get; set; } = () => (TracingId) Guid.NewGuid();

        /// <summary>
        ///     Contains stack of spans for current async flow.
        /// </summary>
        /// <exception cref="TraceIsNotInitialized">
        ///     Trace was not initialized. The best option is to initialize it at the beginning
        ///     of the thread.
        /// </exception>
        [NotNull]
        internal List<TracerSpan> Trace => _traceStorage.Value ?? throw new TraceIsNotInitialized();

        /// <inheritdoc />
        public event SpanClosedDelegate SpanClosed;

        /// <inheritdoc />
        public event SpanOpenedDelegate SpanOpened;

        public Tracer([NotNull] ILogger<Tracer> logger, [NotNull] IOptions<LoggerPropertiesOptions> loggerPropertiesOptions)
        {
            _logger                  = logger;
            _loggerPropertiesOptions = loggerPropertiesOptions.Value.AsNotNull();
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (_traceStorage.Value == null)
                return;

            foreach (var span in _traceStorage.Value.ToNotNullList())
                span.Close();

            _traceStorage.Value = null;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            _traceStorage.Value = new List<TracerSpan>();
        }

        /// <inheritdoc />
        public ITracerSpan OpenChildSpan(string name, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            if (Trace.Count == 0)
                throw new RootSpanIsNotOpened();

            var spanDescriptor = new SpanDescriptor(CurrentSpan.Descriptor.TraceId,
                                                    NewTracingIdGenerator(),
                                                    name,
                                                    CurrentSpan.Descriptor.Id,
                                                    type);

            var span = new TracerSpan(spanDescriptor, SpanOnClosed);

            Trace.Add(span);

            span.AttachResource(_logger.BeginScope(GetLoggerProperties(span))
                                       .AsNotNull());

            InvokeSpanOpened(spanDescriptor);

            return span;
        }

        /// <inheritdoc />
        public ITracerSpan OpenChildSpan(Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenChildSpan(creatorType.FullName.AsNotNull(), type);
        }

        /// <inheritdoc />
        public ITracerSpan OpenRootSpan(
            TracingId traceId,
            TracingId id,
            string name,
            TracingId? parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            if (Trace.Count > 0)
                throw new RootSpanIsAlreadyOpened();

            var spanDescriptor = new SpanDescriptor(traceId,
                                                    id,
                                                    name,
                                                    parentId,
                                                    type);

            var span = new TracerSpan(spanDescriptor, SpanOnClosed);

            Trace.Add(span);

            span.AttachResource(_logger.BeginScope(GetLoggerProperties(span))
                                       .AsNotNull());

            InvokeSpanOpened(spanDescriptor);

            return span;
        }

        /// <inheritdoc />
        public ITracerSpan OpenRootSpan(
            TracingId traceId,
            TracingId id,
            Type creatorType,
            TracingId? parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenRootSpan(traceId,
                                id,
                                creatorType.FullName.AsNotNull(),
                                parentId,
                                type);
        }

        /// <inheritdoc />
        public ITracerSpan OpenRootSpan(string name, SpanType type)
        {
            var newSpanIdentifier = NewTracingIdGenerator();

            return OpenRootSpan(newSpanIdentifier,
                                newSpanIdentifier,
                                name,
                                null,
                                type);
        }

        /// <inheritdoc />
        public ITracerSpan OpenRootSpan(Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenRootSpan(creatorType.FullName.AsNotNull(), type);
        }

        [NotNull]
        private readonly ILogger<Tracer> _logger;

        [NotNull]
        private readonly LoggerPropertiesOptions _loggerPropertiesOptions;

        [NotNull]
        private readonly AsyncLocal<List<TracerSpan>> _traceStorage = new AsyncLocal<List<TracerSpan>>();

        [NotNull]
        private Dictionary<string, object> GetLoggerProperties(TracerSpan span)
        {
            var dictionary = new SelfDescribingDictionary<string, object>();

            foreach (var property in _loggerPropertiesOptions.TraceId.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, span.Descriptor.TraceId);

            foreach (var property in _loggerPropertiesOptions.SpanId.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, span.Descriptor.Id);

            if (span.Descriptor.ParentId != null)
                foreach (var property in _loggerPropertiesOptions.ParentSpanId.Distinct()
                                                                 .AsCollectionWithNotNullItems())
                    dictionary.Add(property, span.Descriptor.ParentId);

            foreach (var property in _loggerPropertiesOptions.SpanName.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, span.Descriptor.Name);

            foreach (var property in _loggerPropertiesOptions.SpanType.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, span.Descriptor.Type);

            return dictionary;
        }

        private void InvokeSpanClosed(SpanDescriptor span, TimeSpan duration)
        {
            SpanClosed?.Invoke(span, duration);
        }

        private void InvokeSpanOpened(SpanDescriptor span)
        {
            SpanOpened?.Invoke(span);
        }

        private void SpanOnClosed(TracerSpan span)
        {
            if (!CurrentSpan.Equals(span))
                _logger.LogWarning("Tracer span that being closed is not the current one.");

            Trace.Remove(span);

            InvokeSpanClosed(span.Descriptor, span.Duration);
        }
    }
}
