using System;
using System.Diagnostics.CodeAnalysis;
using GS.DecoupleIt.Shared;
using Xunit;

namespace GS.DecoupleIt.Tracing.Tests
{
    [ExcludeFromCodeCoverage]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class TracerTests
    {
        [JetBrains.Annotations.NotNull]
        private const string SpanName = "OwnSpan";

        [JetBrains.Annotations.NotNull]
        private static Type CreatorType => typeof(TracerTests);

        [JetBrains.Annotations.NotNull]
        private static ITracer CreateTracer()
        {
            return new Tracer();
        }

        [Fact]
        public void CanTryToDisposeClosedSpan()
        {
            var tracer = CreateTracer();

            var span = tracer.OpenSpan(CreatorType, SpanType.ExternalRequest);

            span.Dispose();
        }

        [Fact]
        public void GetCurrentSpan()
        {
            var tracer = CreateTracer();

            using (tracer.OpenSpan(CreatorType, SpanType.ExternalRequest))
            {
                var span = tracer.CurrentSpan.AsNotNull();

                Assert.NotEqual<TracingId>(Guid.Empty, span.Descriptor.Id);
                Assert.NotEqual<TracingId>(Guid.Empty, span.Descriptor.TraceId);
                Assert.Null(span.Descriptor.ParentId);
                Assert.NotNull(span.Descriptor.Name);
                Assert.Equal(SpanType.ExternalRequest, span.Descriptor.Type);
            }
        }

        [Fact]
        public void OpenChildSpan_WithRootSpan()
        {
            var tracer = CreateTracer();

            var rootSpan  = tracer.OpenSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = tracer.OpenSpan(CreatorType, SpanType.ExternalRequest);
        }

        [Fact]
        public void OpenSpan_NoParent_CreatorTypeAsName()
        {
            var tracer = CreateTracer();

            using (var _ = tracer.OpenSpan(CreatorType, SpanType.ExternalRequest)) { }
        }

        [Fact]
        public void OpenSpan_NoParent_OwnName()
        {
            var tracer = CreateTracer();

            using (var _ = tracer.OpenSpan(SpanName, SpanType.ExternalRequest)) { }
        }

        [Fact]
        public void OpenSpan_WithParent_CreatorTypeAsName()
        {
            var tracer = CreateTracer();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var _ = tracer.OpenSpan(traceId,
                                           spanId,
                                           creatorType,
                                           parentSpanId,
                                           SpanType.ExternalRequest)) { }
        }

        [Fact]
        public void OpenSpan_WithParent_OwnName()
        {
            var tracer = CreateTracer();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = tracer.OpenSpan(traceId,
                                           spanId,
                                           spanName,
                                           parentSpanId,
                                           SpanType.ExternalRequest)) { }
        }
    }
}
