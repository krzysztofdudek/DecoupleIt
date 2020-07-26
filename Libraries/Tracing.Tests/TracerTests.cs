using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing.Exceptions;
using NSubstitute;
using NUnit.Framework;

namespace GS.DecoupleIt.Tracing.Tests
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public class TracerTests
    {
        [SetUp]
        public void SetUp()
        {
            Tracer.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            Tracer.Clear();
        }

        [JetBrains.Annotations.NotNull]
        private const string SpanName = "OwnSpan";

        [JetBrains.Annotations.NotNull]
        private static Type CreatorType => typeof(TracerTests);

        [Test]
        public void CanNotDisposeNotCurrentSpan()
        {
            var rootSpan  = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);

            Assert.Throws<InvalidOperationException>(() => { rootSpan.Dispose(); });
        }

        [Test]
        public void CanTryToDisposeClosedSpan()
        {
            var span = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);

            span.Dispose();
        }

        [Test]
        public void ClearInitialized()
        {
            Tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });
        }

        [Test]
        public void DisposeResourcesOnSpanClose()
        {
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

            Assert.IsTrue(isDisposed);
        }

        [Test]
        public void get_CurrentSpan_NotInTheContextOfSpan()
        {
            Assert.Throws<NotInTheContextOfSpan>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });
        }

        [Test]
        public void get_CurrentSpan_TraceIsNotInitialized()
        {
            Tracer.Clear();

            Assert.Throws<TraceIsNotInitialized>(() =>
            {
                var _ = Tracer.CurrentSpan;
            });
        }

        [Test]
        public void GetCurrentSpan()
        {
            using (Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                var span = Tracer.CurrentSpan;

                Assert.AreNotEqual(Guid.Empty, span.Id);
                Assert.AreNotEqual(Guid.Empty, span.TraceId);
                Assert.Null(span.ParentId);
                Assert.NotNull(span.Name);
                Assert.AreEqual(SpanType.ExternalRequest, span.Type);
            }
        }

        [Test]
        public void GetResource_ExistsTyped()
        {
            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder());

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    var resource = Tracer.GetResource<StringBuilder>();

                    Assert.IsNotNull(resource);
                }
            }
        }

        [Test]
        public void GetResource_ExistsTypedAndKeyed()
        {
            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder(), "Key");

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    var resource = Tracer.GetResource<StringBuilder>("Key");

                    Assert.IsNotNull(resource);
                }
            }
        }

        [Test]
        public void GetResource_InvalidKey()
        {
            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new StringBuilder(), "Key1");

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    Assert.Throws<ResourceNotFound>(() => { Tracer.GetResource<StringBuilder>("Key2"); });
                }
            }
        }

        [Test]
        public void GetResource_InvalidType()
        {
            using (var rootSpan = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest))
            {
                rootSpan.AttachResource(new List());

                using (var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequestHandler))
                {
                    Assert.Throws<ResourceNotFound>(() => { Tracer.GetResource<StringBuilder>(); });
                }
            }
        }


        [Test]
        public void HandleMetricOnSpanLevel()
        {
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
        }

        [Test]
        public void HandleMetricOnStaticLevel()
        {
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
        }

        [Test]
        public void InitializationOfMetric()
        {
            const string  name  = "name";
            const decimal value = 100;

            var metric = new Metric(name, value);

            Assert.AreEqual(name, metric.Name);
            Assert.AreEqual(value, metric.Value);
        }

        [Test]
        public void IsNotRootSpanOpened()
        {
            Assert.False(Tracer.IsRootSpanOpened);
        }

        [Test]
        public void IsRootSpanOpened()
        {
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
        }

        [Test]
        public void OpenChildSpan_NoParent_CreatorTypeAsName()
        {
            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest)) { }
            });
        }

        [Test]
        public void OpenChildSpan_NoParent_OwnName()
        {
            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                using (var _ = Tracer.OpenChildSpan(SpanName, SpanType.ExternalRequest)) { }
            });
        }

        [Test]
        public void OpenChildSpan_NoRootSpan()
        {
            Assert.Throws<RootSpanIsNotOpened>(() =>
            {
                var _ = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);
            });
        }

        [Test]
        public void OpenChildSpan_WithRootSpan()
        {
            var rootSpan  = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest);
            var childSpan = Tracer.OpenChildSpan(CreatorType, SpanType.ExternalRequest);
        }

        [Test]
        public void OpenRootSpan_NoParent_CreatorTypeAsName()
        {
            using (var _ = Tracer.OpenRootSpan(CreatorType, SpanType.ExternalRequest)) { }
        }

        [Test]
        public void OpenRootSpan_NoParent_OwnName()
        {
            using (var _ = Tracer.OpenRootSpan(SpanName, SpanType.ExternalRequest)) { }
        }

        [Test]
        public void OpenRootSpan_RootSpanAlreadyDefined()
        {
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
        }

        [Test]
        public void OpenRootSpan_WithParent_CreatorTypeAsName()
        {
            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var creatorType  = CreatorType;
            var parentSpanId = Guid.NewGuid();

            using (var _ = Tracer.OpenRootSpan(traceId,
                                               spanId,
                                               creatorType,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }
        }

        [Test]
        public void OpenRootSpan_WithParent_OwnName()
        {
            var traceId      = Guid.NewGuid();
            var spanId       = Guid.NewGuid();
            var spanName     = SpanName;
            var parentSpanId = Guid.NewGuid();

            using (var _ = Tracer.OpenRootSpan(traceId,
                                               spanId,
                                               spanName,
                                               parentSpanId,
                                               SpanType.ExternalRequest)) { }
        }
    }
}
