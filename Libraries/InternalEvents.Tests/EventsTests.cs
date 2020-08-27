using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.InternalEvents.Scope;
using GS.DecoupleIt.Tracing;
using Xunit;

#pragma warning disable 1998

namespace GS.DecoupleIt.InternalEvents.Tests
{
    public class EventsTests
    {
        private sealed class ExampleEvent : Event { }

        private sealed class AnotherEvent : Event { }

        [Fact]
        public void AggregateEvents()
        {
            var                        event1 = new ExampleEvent();
            var                        event2 = new ExampleEvent();
            IReadOnlyCollection<Event> events = null;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                InternalEventsScope.EmitEvent(event1);

                InternalEventsScope.AggregateEvents(() => { InternalEventsScope.EmitEvent(event2); },
                                                    aggregatedEvents => { events = aggregatedEvents; },
                                                    typeof(ExampleEvent));
            }

            Tracer.Clear();

            Assert.DoesNotContain(event1, events);
            Assert.Contains(event2, events.ToList());
        }

        [Fact]
        public async Task AggregateEventsAsync()
        {
            var                        event1 = new ExampleEvent();
            var                        event2 = new ExampleEvent();
            IReadOnlyCollection<Event> events = null;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                await InternalEventsScope.EmitEventAsync(event1);

                await InternalEventsScope.AggregateEventsAsync(async () => { await InternalEventsScope.EmitEventAsync(event2); },
                                                               async aggregatedEvents => { events = aggregatedEvents; },
                                                               typeof(ExampleEvent));
            }

            Tracer.Clear();

            Assert.DoesNotContain(event1, events);
            Assert.Contains(event2, events.ToList());
        }

        [Fact]
        public void AggregateEventsFromInnerScopes()
        {
            var event1 = new ExampleEvent();

            IReadOnlyCollection<Event> events = null;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
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

            Tracer.Clear();

            Assert.Contains(event1, events.ToList());
        }

        [Fact]
        public void AggregateSelectedEvents()
        {
            var event1 = new ExampleEvent();
            var event2 = new AnotherEvent();

            IReadOnlyCollection<Event> events = null;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (InternalEventsScope.OpenScope())
            {
                InternalEventsScope.EmitEvent(event1);

                InternalEventsScope.AggregateEvents(() => { InternalEventsScope.EmitEvent(event2); },
                                                    aggregatedEvents => { events = aggregatedEvents; },
                                                    typeof(AnotherEvent));
            }

            Tracer.Clear();

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

            var emitTask = Task.Run(async () =>
            {
                Tracer.Initialize();

                using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
                using (var scope = InternalEventsScope.OpenScope())
                {
                    scope.EventEmitted += (eventsScope, @event, token) =>
                    {
                        emittedEvent = (ExampleEvent) @event;

                        return Task.CompletedTask;
                    };

                    while (!emit)
                        await Task.Delay(20);

                    await InternalEventsScope.EmitEventAsync(event1);
                }

                Tracer.Clear();
            });

            var aggregateTask = Task.Run(() =>
            {
                Tracer.Initialize();

                using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
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

                Tracer.Clear();
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

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (scope = InternalEventsScope.OpenScope())
            {
                scope.EventEmitted += TracerOnEventEmitted;

                InternalEventsScope.EmitEvent(@event);

                scope.EventEmitted -= TracerOnEventEmitted;
            }

            Tracer.Clear();

            Assert.True(wasEventEmitted);

            Assert.Contains(@event, scope.Events.ToList());
        }

        [Fact]
        public void HandleEventOnStaticLevel()
        {
            var wasEventEmitted = false;

            var @event = new ExampleEvent();

            Task TracerOnEventEmitted(IInternalEventsScope scope2, Event _1, CancellationToken _2)
            {
                wasEventEmitted = true;

                return Task.CompletedTask;
            }

            InternalEventsScope.EventEmitted += TracerOnEventEmitted;

            IInternalEventsScope scope;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (scope = InternalEventsScope.OpenScope())
            {
                InternalEventsScope.EmitEvent(@event);
            }

            Tracer.Clear();

            InternalEventsScope.EventEmitted -= TracerOnEventEmitted;

            Assert.True(wasEventEmitted);

            Assert.Contains(@event, scope.Events.ToList());
        }

        [Fact]
        public async Task HandleEventOnStaticLevelEmittedAsynchonously()
        {
            var wasEventEmitted = false;

            var @event = new ExampleEvent();

            Task TracerOnEventEmitted(IInternalEventsScope scope2, Event _1, CancellationToken _2)
            {
                wasEventEmitted = true;

                return Task.CompletedTask;
            }

            InternalEventsScope.EventEmitted += TracerOnEventEmitted;

            IInternalEventsScope scope;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            using (scope = InternalEventsScope.OpenScope())
            {
                await InternalEventsScope.EmitEventAsync(@event);
            }

            Tracer.Clear();

            InternalEventsScope.EventEmitted -= TracerOnEventEmitted;

            Assert.True(wasEventEmitted);

            Assert.Contains(@event, scope.Events.ToList());
        }

        [Fact]
        public void InitializationOfEvent()
        {
            var @event = new ExampleEvent();

            Assert.NotEqual(default, @event.Identifier);
            Assert.NotEqual(default, @event.TimeStamp);
        }
    }
}
