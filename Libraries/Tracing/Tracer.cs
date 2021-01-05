using System;
using System.Collections.Generic;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Tracing
{
    [PublicAPI]
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    internal sealed class Tracer : ITracer
    {
        public ITracerSpan CurrentSpan => _spanStorage.Value;

        public Func<TracingId> NewTracingIdGenerator
        {
            get => _newTracingIdGenerator;
            set
            {
                ContractGuard.IfArgumentIsNull(nameof(value), value);

                _newTracingIdGenerator = value;
            }
        }

        public event SpanClosedDelegate SpanClosed;

        public event SpanOpenedDelegate SpanOpened;

        public Tracer([CanBeNull] ILogger<Tracer> logger = default, [CanBeNull] IOptions<LoggerPropertiesOptions> loggerPropertiesOptions = default)
        {
            _logger                  = logger;
            _loggerPropertiesOptions = loggerPropertiesOptions?.Value.AsNotNull();
        }

        public ITracerSpan OpenSpan(string name, SpanType type)
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

        public ITracerSpan OpenSpan(Type creatorType, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(creatorType), creatorType);

            return OpenSpan(creatorType.FullName!, type);
        }

        public ITracerSpan OpenSpan(
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

            var span = new TracerSpan(spanDescriptor,
                                      CurrentSpan,
                                      SpanOnClosed,
                                      _logger?.BeginScope(GetLoggerProperties(spanDescriptor)));

            CurrentSpanInternal = span;

            InvokeSpanOpened(spanDescriptor);

            return span;
        }

        public ITracerSpan OpenSpan(
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

        [CanBeNull]
        private readonly ILogger<Tracer> _logger;

        [CanBeNull]
        private readonly LoggerPropertiesOptions _loggerPropertiesOptions;

        [NotNull]
        private Func<TracingId> _newTracingIdGenerator = () => (TracingId) Guid.NewGuid();

        [NotNull]
        private readonly AsyncLocal<ITracerSpan> _spanStorage = new();

        private TracerSpan? CurrentSpanInternal
        {
            get => (TracerSpan?) _spanStorage.Value;
            set => _spanStorage.Value = value;
        }

        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private IReadOnlyDictionary<string, object> GetLoggerProperties(SpanDescriptor descriptor)
        {
            if (_loggerPropertiesOptions is null)
                return new Dictionary<string, object>();

            var dictionary = new SelfDescribingDictionary<string, object>();

            for (var index = 0; index < _loggerPropertiesOptions.TraceId.Count; index++)
            {
                var property = _loggerPropertiesOptions.TraceId[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.TraceId);
            }

            for (var index = 0; index < _loggerPropertiesOptions.SpanId.Count; index++)
            {
                var property = _loggerPropertiesOptions.SpanId[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.Id);
            }

            if (descriptor.ParentId != null)
                for (var index = 0; index < _loggerPropertiesOptions.ParentSpanId.Count; index++)
                {
                    var property = _loggerPropertiesOptions.ParentSpanId[index];

                    if (!dictionary.ContainsKey(property))
                        dictionary.Add(property, descriptor.ParentId);
                }

            for (var index = 0; index < _loggerPropertiesOptions.SpanName.Count; index++)
            {
                var property = _loggerPropertiesOptions.SpanName[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.Name);
            }

            for (var index = 0; index < _loggerPropertiesOptions.SpanType.Count; index++)
            {
                var property = _loggerPropertiesOptions.SpanType[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.Type);
            }

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
            if (CurrentSpan is null)
            {
                _logger?.LogWarning("There is no span attached to current flow. Perhaps it was already closed before.");

                return;
            }

            if (!((TracerSpan) CurrentSpan).Equals(span))
            {
                _logger?.LogWarning("Current tracer span is different than the one that is being closed.");

                return;
            }

            if (CurrentSpan.Parent != null)
                CurrentSpanInternal = (TracerSpan) CurrentSpan.Parent;
            else
                CurrentSpanInternal = null;

            InvokeSpanClosed(span.Descriptor, span.Duration);
        }
    }
}
