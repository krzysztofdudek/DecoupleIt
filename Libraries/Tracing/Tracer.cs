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
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    internal sealed class Tracer : ITracer
    {
        public ITracerSpan CurrentSpan => _spanStorage.Value;

        public event SpanClosedDelegate SpanClosed;

        public event SpanOpenedDelegate SpanOpened;

        public Tracer([NotNull] ILogger<Tracer> logger, [NotNull] IOptions<Options> options)
        {
            _logger  = logger;
            _options = options.Value!;
        }

        public ITracerSpan OpenSpan(string name, SpanType type)
        {
            ContractGuard.IfArgumentIsNull(nameof(name), name);
            ContractGuard.IfEnumArgumentIsOutOfRange(nameof(type), type);

            var spanId       = _options.NewTracingIdGenerator();
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
                                      _logger.BeginScope(GetLoggerProperties(spanDescriptor)));

            _spanStorage.Value = span;

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

        [NotNull]
        private readonly ILogger<Tracer> _logger;

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly AsyncLocal<ITracerSpan> _spanStorage = new();

        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
        private IReadOnlyDictionary<string, object> GetLoggerProperties(SpanDescriptor descriptor)
        {
            var dictionary = new Dictionary<string, object>();

            for (var index = 0; index < _options.LoggerProperties.TraceId.Count; index++)
            {
                var property = _options.LoggerProperties.TraceId[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.TraceId);
            }

            for (var index = 0; index < _options.LoggerProperties.SpanId.Count; index++)
            {
                var property = _options.LoggerProperties.SpanId[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.Id);
            }

            if (descriptor.ParentId != null)
                for (var index = 0; index < _options.LoggerProperties.ParentSpanId.Count; index++)
                {
                    var property = _options.LoggerProperties.ParentSpanId[index];

                    if (!dictionary.ContainsKey(property))
                        dictionary.Add(property, descriptor.ParentId);
                }

            for (var index = 0; index < _options.LoggerProperties.SpanName.Count; index++)
            {
                var property = _options.LoggerProperties.SpanName[index];

                if (!dictionary.ContainsKey(property))
                    dictionary.Add(property, descriptor.Name);
            }

            for (var index = 0; index < _options.LoggerProperties.SpanType.Count; index++)
            {
                var property = _options.LoggerProperties.SpanType[index];

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
                _logger.LogWarning("There is no span attached to current flow. Perhaps it was already closed before.");

                return;
            }

            if (!((TracerSpan) CurrentSpan).Equals(span))
            {
                _logger.LogWarning("Current tracer span is different than the one that is being closed.");

                return;
            }

            if (CurrentSpan.Parent != null)
                _spanStorage.Value = (TracerSpan) CurrentSpan.Parent;
            else
                _spanStorage.Value = null;

            InvokeSpanClosed(span.Descriptor, span.Duration);
        }
    }
}
