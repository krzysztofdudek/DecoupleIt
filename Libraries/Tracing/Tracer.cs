using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
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
        public TracerSpan? CurrentSpan
        {
            get => _spanStorage.Value;
            private set => _spanStorage.Value = value;
        }

        /// <inheritdoc />
        public Func<TracingId> NewTracingIdGenerator { get; set; } = () => (TracingId) Guid.NewGuid();

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
        public TracerSpan OpenSpan(string name, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            var spanId       = NewTracingIdGenerator();
            var traceId      = CurrentSpan?.Descriptor.TraceId ?? spanId;
            var parentSpanId = CurrentSpan?.Descriptor.Id;

            return OpenSpan(traceId,
                            spanId,
                            name,
                            parentSpanId,
                            type);
        }

        /// <inheritdoc />
        public TracerSpan OpenSpan(Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenSpan(creatorType.FullName.AsNotNull(), type);
        }

        /// <inheritdoc />
        public TracerSpan OpenSpan(
            TracingId traceId,
            TracingId id,
            string name,
            TracingId? parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            var spanDescriptor = new SpanDescriptor(traceId,
                                                    id,
                                                    name,
                                                    parentId,
                                                    type);

            var span = new TracerSpan(spanDescriptor, CurrentSpan, SpanOnClosed, _logger.BeginScope(GetLoggerProperties(spanDescriptor))
                                                                                        .AsNotNull());

            CurrentSpan = span;

            InvokeSpanOpened(spanDescriptor);

            return span;
        }

        /// <inheritdoc />
        public TracerSpan OpenSpan(
            TracingId traceId,
            TracingId id,
            Type creatorType,
            TracingId? parentId,
            SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenSpan(traceId,
                            id,
                            creatorType.FullName.AsNotNull(),
                            parentId,
                            type);
        }

        [NotNull]
        private readonly ILogger<Tracer> _logger;

        [NotNull]
        private readonly LoggerPropertiesOptions _loggerPropertiesOptions;

        [NotNull]
        private readonly AsyncLocal<TracerSpan?> _spanStorage = new AsyncLocal<TracerSpan?>();

        [NotNull]
        private IReadOnlyDictionary<string, object> GetLoggerProperties(SpanDescriptor descriptor)
        {
            var dictionary = new SelfDescribingDictionary<string, object>();

            foreach (var property in _loggerPropertiesOptions.TraceId.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, descriptor.TraceId);

            foreach (var property in _loggerPropertiesOptions.SpanId.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, descriptor.Id);

            if (descriptor.ParentId != null)
                foreach (var property in _loggerPropertiesOptions.ParentSpanId.Distinct()
                                                                 .AsCollectionWithNotNullItems())
                    dictionary.Add(property, descriptor.ParentId);

            foreach (var property in _loggerPropertiesOptions.SpanName.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, descriptor.Name);

            foreach (var property in _loggerPropertiesOptions.SpanType.Distinct()
                                                             .AsCollectionWithNotNullItems())
                dictionary.Add(property, descriptor.Type);

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
