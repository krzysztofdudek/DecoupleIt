using System;
using System.Diagnostics.CodeAnalysis;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
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
        private ITracer CreateTracer()
        {
            return new Tracer(new NullLogger<Tracer>(),
                              Microsoft.Extensions.Options.Options.Create(new LoggerPropertiesOptions())
                                       .AsNotNull());
        }

        [Fact]
        public void CanTryToDisposeClosedSpan()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var span = tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);

            span.Dispose();

            tracer.Clear();
        }

        [Fact]
        public void ClearInitialized()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = tracer.CurrentSpan;
            });

            tracer.Clear();
        }

        [Fact]
        public void DisposeResourcesOnSpanClose()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var isDisposed = false;

            var disposable = Substitute.For<IDisposable>()
                                       .AsNotNull();

            disposable.When(x => x.AsNotNull()
                                  .Dispose())
                      .AsNotNull()
                      .Do(_ => isDisposed = true);

            using (var span = tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                span.AttachResource(disposable);
            }

            Assert.True(isDisposed);

            tracer.Clear();
        }

        [Fact]
        public void get_CurrentSpan_NotInTheContextOfSpan()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            Assert.Throws<NotInTheContextOfSpan>(() =>
            {
                var _ = tracer.CurrentSpan;
            });

            tracer.Clear();
        }

        [Fact]
        public void get_CurrentSpan_TraceIsNotInitialized()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = tracer.CurrentSpan;
            });

            tracer.Clear();
        }

        [Fact]
        public void GetCurrentSpan()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                var span = tracer.CurrentSpan;

                Assert.NotEqual<TracingId>(Guid.Empty, span.Descriptor.Id);
                Assert.NotEqual<TracingId>(Guid.Empty, span.Descriptor.TraceId);
                Assert.Null(span.Descriptor.ParentId);
                Assert.NotNull(span.Descriptor.Name);
                Assert.Equal(SpanType.ExternalRequest, span.Descriptor.Type);
            }

            tracer.Clear();
        }

        [Fact]
        public void IsNotRootSpanOpened()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            Assert.False(tracer.IsRootSpanOpened);

            tracer.Clear();
        }

        [Fact]
        public void IsRootSpanOpened()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = tracer.OpenRootSpan(traceId,
                                               spanId,
                                               spanName,
                                               parentSpanId,
                                               SpanType.ExternalRequest))
            {
                Assert.True(tracer.IsRootSpanOpened);
            }

            tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoParent_CreatorTypeAsName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest)) { }
            });

            tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoParent_OwnName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = tracer.OpenChildSpan(SpanName, SpanType.ExternalRequest)) { }
            });

            tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoRootSpan()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                var _ = tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);
            });

            tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_WithRootSpan()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var rootSpan  = tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);

            tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_NoParent_CreatorTypeAsName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            using (var _ = tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest)) { }

            tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_NoParent_OwnName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            using (var _ = tracer.OpenRootSpan(SpanName, SpanType.ExternalRequest)) { }

            tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_RootSpanAlreadyDefined()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var rootSpan = tracer.OpenRootSpan(traceId,
                                                      spanId,
                                                      creatorType,
                                                      parentSpanId,
                                                      SpanType.ExternalRequest))
            {
                Assert.Throws<RootSpanIsAlreadyOpened>(() =>
                {
                    using (var _ = tracer.OpenRootSpan(traceId,
                                                       spanId,
                                                       creatorType,
                                                       parentSpanId,
                                                       SpanType.ExternalRequest)) { }
                });
            }

            tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_WithParent_CreatorTypeAsName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var _ = tracer.OpenRootSpan(traceId,
                                               spanId,
                                               creatorType,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }

            tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_WithParent_OwnName()
        {
            var tracer = CreateTracer();

            tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = tracer.OpenRootSpan(traceId,
                                               spanId,
                                               spanName,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }

            tracer.Clear();
        }
    }
}
