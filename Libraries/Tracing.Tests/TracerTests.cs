using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing.Exceptions;
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

        [Fact]
        public void CanNotDisposeNotCurrentSpan()
        {
            Tracer.Initialize();

            var rootSpan  = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);

            Assert.Throws<InvalidOperationException>(() => { rootSpan.Dispose(); });

            Tracer.Clear();
        }

        [Fact]
        public void CanTryToDisposeClosedSpan()
        {
            Tracer.Initialize();

            var span = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);

            span.Dispose();

            Tracer.Clear();
        }

        [Fact]
        public void ClearInitialized()
        {
            Tracer.Initialize();

            Tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });

            Tracer.Clear();
        }

        [Fact]
        public void DisposeResourcesOnSpanClose()
        {
            Tracer.Initialize();

            var isDisposed = false;

            var disposable = Substitute.For<IDisposable>()
                                       .AsNotNull();

            disposable.When(x => x.AsNotNull()
                                  .Dispose())
                      .AsNotNull()
                      .Do(_ => isDisposed = true);

            using (Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                Tracer.AttachResource(disposable);
            }

            Assert.True(isDisposed);

            Tracer.Clear();
        }

        [Fact]
        public void get_CurrentSpan_NotInTheContextOfSpan()
        {
            Tracer.Initialize();

            Assert.Throws<NotInTheContextOfSpan>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });

            Tracer.Clear();
        }

        [Fact]
        public void get_CurrentSpan_TraceIsNotInitialized()
        {
            Tracer.Initialize();

            Tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });

            Tracer.Clear();
        }

        [Fact]
        public void GetCurrentSpan()
        {
            Tracer.Initialize();

            using (Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                var span = Tracer.CurrentSpan;

                Assert.NotEqual(Guid.Empty, span.Id);
                Assert.NotEqual(Guid.Empty, span.TraceId);
                Assert.Null(span.ParentId);
                Assert.NotNull(span.Name);
                Assert.Equal(SpanType.ExternalRequest, span.Type);
            }

            Tracer.Clear();
        }

        [Fact]
        public void GetResource_ExistsTyped()
        {
            Tracer.Initialize();

            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder());

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    var resource = Tracer.GetResource<StringBuilder>();

                    Assert.NotNull(resource);
                }
            }

            Tracer.Clear();
        }

        [Fact]
        public void GetResource_ExistsTypedAndKeyed()
        {
            Tracer.Initialize();

            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder(), "Key");

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    var resource = Tracer.GetResource<StringBuilder>("Key");

                    Assert.NotNull(resource);
                }
            }

            Tracer.Clear();
        }

        [Fact]
        public void GetResource_InvalidKey()
        {
            Tracer.Initialize();

            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder(), "Key1");

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    Assert.Throws<ResourceNotFound>(() => { Tracer.GetResource<StringBuilder>("Key2"); });
                }
            }

            Tracer.Clear();
        }

        [Fact]
        public void GetResource_InvalidType()
        {
            Tracer.Initialize();

            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new List<StringBuilder>());

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    Assert.Throws<ResourceNotFound>(() => { Tracer.GetResource<StringBuilder>(); });
                }
            }

            Tracer.Clear();
        }

        [Fact]
        public void HandleMetricOnSpanLevel()
        {
            Tracer.Initialize();

            var wasMetricPushed = false;

            var metric = new Metric("Name", 1);

            void TracerOnMetricPushed(Span _, Metric __)
            {
                wasMetricPushed = true;
            }

            Tracer.Initialize();

            ISpan span;

            using (span = Tracer.OpenRootSpan(typeof(TracerTests), SpanType.ExternalRequest))
            {
                span.MetricPushed += TracerOnMetricPushed;

                Tracer.PushMetric(metric);

                span.MetricPushed -= TracerOnMetricPushed;
            }

            Tracer.Clear();

            Assert.True(wasMetricPushed);

            Assert.Contains(metric, span.Metrics.ToList());

            Tracer.Clear();
        }

        [Fact]
        public void HandleMetricOnStaticLevel()
        {
            Tracer.Initialize();

            var wasMetricPushed = false;

            var metric = new Metric("Name", 1);

            void TracerOnMetricPushed(Span _, Metric __)
            {
                wasMetricPushed = true;
            }

            Tracer.Initialize();
            Tracer.MetricPushed += TracerOnMetricPushed;

            ISpan span;

            using (span = Tracer.OpenRootSpan(typeof(TracerTests), SpanType.ExternalRequest))
            {
                Tracer.PushMetric(metric);
            }

            Tracer.MetricPushed -= TracerOnMetricPushed;
            Tracer.Clear();

            Assert.True(wasMetricPushed);

            Assert.Contains(metric, span.Metrics.ToList());

            Tracer.Clear();
        }

        [Fact]
        public void InitializationOfMetric()
        {
            Tracer.Initialize();

            const string  name  = "name";
            const decimal value = 100;

            var metric = new Metric(name, value);

            Assert.Equal(name, metric.Name);
            Assert.Equal(value, metric.Value);

            Tracer.Clear();
        }

        [Fact]
        public void IsNotRootSpanOpened()
        {
            Tracer.Initialize();

            Assert.False(Tracer.IsRootSpanOpened);

            Tracer.Clear();
        }

        [Fact]
        public void IsRootSpanOpened()
        {
            Tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = Tracer.OpenRootSpan(traceId,
                                               spanId,
                                               spanName,
                                               parentSpanId,
                                               SpanType.ExternalRequest))
            {
                Assert.True(Tracer.IsRootSpanOpened);
            }

            Tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoParent_CreatorTypeAsName()
        {
            Tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest)) { }
            });

            Tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoParent_OwnName()
        {
            Tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = Tracer.OpenChildSpan(SpanName, SpanType.ExternalRequest)) { }
            });

            Tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_NoRootSpan()
        {
            Tracer.Initialize();

            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                var _ = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);
            });

            Tracer.Clear();
        }

        [Fact]
        public void OpenChildSpan_WithRootSpan()
        {
            Tracer.Initialize();

            var rootSpan  = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);

            Tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_NoParent_CreatorTypeAsName()
        {
            Tracer.Initialize();

            using (var _ = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest)) { }

            Tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_NoParent_OwnName()
        {
            Tracer.Initialize();

            using (var _ = Tracer.OpenRootSpan(SpanName, SpanType.ExternalRequest)) { }

            Tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_RootSpanAlreadyDefined()
        {
            Tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var rootSpan = Tracer.OpenRootSpan(traceId,
                                                      spanId,
                                                      creatorType,
                                                      parentSpanId,
                                                      SpanType.ExternalRequest))
            {
                Assert.Throws<RootSpanIsAlreadyOpened>(() =>
                {
                    using (var _ = Tracer.OpenRootSpan(traceId,
                                                       spanId,
                                                       creatorType,
                                                       parentSpanId,
                                                       SpanType.ExternalRequest)) { }
                });
            }

            Tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_WithParent_CreatorTypeAsName()
        {
            Tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var _ = Tracer.OpenRootSpan(traceId,
                                               spanId,
                                               creatorType,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }

            Tracer.Clear();
        }

        [Fact]
        public void OpenRootSpan_WithParent_OwnName()
        {
            Tracer.Initialize();

            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = Tracer.OpenRootSpan(traceId,
                                               spanId,
                                               spanName,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }

            Tracer.Clear();
        }
    }
}
