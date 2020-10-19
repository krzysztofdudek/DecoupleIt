using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#pragma warning disable 1998

namespace GS.DecoupleIt.InternalEvents.Tests
{
    public class EventsTests
    {
        private sealed class ExampleEvent : Event { }

        private sealed class AnotherEvent : Event { }

        [NotNull]
        private static ITracer CreateTracer()
        {
            return new Tracer(new NullLogger<Tracer>(),
                              Microsoft.Extensions.Options.Options.Create(new LoggerPropertiesOptions())
                                       .AsNotNull());
        }

        [Fact]
        public void AggregateEvents()
        {
            var                        event1 = new ExampleEvent();
            var                        event2 = new ExampleEvent();
            IReadOnlyCollection<Event> events = null;

            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                InternalEventsScope.EmitEvent(event1);

                InternalEventsScope.AggregateEvents(() => { InternalEventsScope.EmitEvent(event2); },
                                                    aggregatedEvents => { events = aggregatedEvents; },
                                                    typeof(ExampleEvent));
            }

            tracer.Clear();

            Assert.DoesNotContain(event1, events);
            Assert.Contains(event2, events.ToList());
        }

        [Fact]
        public async Task AggregateEventsAsync()
        {
            var                        event1 = new ExampleEvent();
            var                        event2 = new ExampleEvent();
            IReadOnlyCollection<Event> events = null;

            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                await InternalEventsScope.EmitEventAsync(event1);

                await InternalEventsScope.AggregateEventsAsync(async () => { await InternalEventsScope.EmitEventAsync(event2); },
                                                               async aggregatedEvents => { events = aggregatedEvents; },
                                                               typeof(ExampleEvent));
            }

            tracer.Clear();

            Assert.DoesNotContain(event1, events);
            Assert.Contains(event2, events.ToList());
        }

        [Fact]
        public void AggregateEventsFromInnerScopes()
        {
            var event1 = new ExampleEvent();

            IReadOnlyCollection<Event> events = null;

            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                InternalEventsScope.AggregateEvents(() =>
                                                    {
                                                        using (InternalEventsScope.OpenScope())
                                                        {
                                                            InternalEventsScope.EmitEvent(event1);
                                                        }
                                                    },
                                                    aggregatedEvents => { events = aggregatedEvents; },
                                                    typeof(ExampleEvent));
            }

            tracer.Clear();

            Assert.Contains(event1, events.ToList());
        }

        [Fact]
        public void AggregateSelectedEvents()
        {
            var event1 = new ExampleEvent();
            var event2 = new AnotherEvent();

            IReadOnlyCollection<Event> events = null;

            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                InternalEventsScope.EmitEvent(event1);

                InternalEventsScope.AggregateEvents(() => { InternalEventsScope.EmitEvent(event2); },
                                                    aggregatedEvents => { events = aggregatedEvents; },
                                                    typeof(AnotherEvent));
            }

            tracer.Clear();

            Assert.DoesNotContain(event1, events);
            Assert.Contains(event2, events.ToList());
        }

        [Fact]
        public void DontAggregateEventsFromAnotherThread()
        {
            var event1 = new ExampleEvent();

            ExampleEvent               emittedEvent = null;
            IReadOnlyCollection<Event> events       = null;
            var                        emit         = false;

            var tracer = CreateTracer();

            var emitTask = Task.Run(async () =>
                               {
                                   tracer.Initialize();

                                   using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
                                   using (var scope = InternalEventsScope.OpenScope())
                                   {
                                       scope.EventEmitted += (eventsScope, @event, token) =>
                                       {
                                           emittedEvent = (ExampleEvent) @event;

                                           return Task.CompletedTask.AsNotNull();
                                       };

                                       while (!emit)
                                           await Task.Delay(20)
                                                     .AsNotNull();

                                       await InternalEventsScope.EmitEventAsync(event1);
                                   }

                                   tracer.Clear();
                               })
                               .AsNotNull();

            var aggregateTask = Task.Run(() =>
            {
                tracer.Initialize();

                using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
                using (InternalEventsScope.OpenScope())
                {
                    InternalEventsScope.AggregateEvents(() =>
                                                        {
                                                            emit = true;

                                                            emitTask.Wait();
                                                        },
                                                        aggregatedEvents => { events = aggregatedEvents; },
                                                        typeof(ExampleEvent));
                }

                tracer.Clear();
            });

            Task.WaitAll(emitTask, aggregateTask);

            Assert.DoesNotContain(event1, events);
            Assert.Equal(event1, emittedEvent);
        }

        [Fact]
        public void HandleEventOnScopeLevel()
        {
            var wasEventEmitted = false;

            var @event = new ExampleEvent();

            Task TracerOnEventEmitted(IInternalEventsScope scope2, Event _1, CancellationToken _2)
            {
                wasEventEmitted = true;

                return Task.CompletedTask;
            }

            IInternalEventsScope scope;

            var tracer = CreateTracer();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (scope = InternalEventsScope.OpenScope())
            {
                scope.EventEmitted += TracerOnEventEmitted;

                InternalEventsScope.EmitEvent(@event);

                scope.EventEmitted -= TracerOnEventEmitted;
            }

            tracer.Clear();

            Assert.True(wasEventEmitted);

            Assert.Contains(@event, scope.Events.ToList());
        }

        [Fact]
        public void InitializationOfEvent()
        {
            var @event = new ExampleEvent();

            Assert.NotEqual(default, @event.EventIdentifier);
            Assert.NotEqual(default, @event.TimeStamp);
        }
    }
}
