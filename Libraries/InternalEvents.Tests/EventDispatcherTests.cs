using System;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    public sealed class EventDispatcherTests
    {
        private static async Task TestEventDispatching([NotNull] Func<IInternalEventsScope, IInternalEventDispatcher, Task> action)
        {
            ExampleEventOnSuccessHandler.HandlesCount  = 0;
            ExampleEventOnFailureHandler.HandlesCount  = 0;
            ExampleEventOnEmissionHandler.HandlesCount = 0;

            ExceptionCausingEventOnSuccessHandler.HandlesCount  = 0;
            ExceptionCausingEventOnFailureHandler.HandlesCount  = 0;
            ExceptionCausingEventOnEmissionHandler.HandlesCount = 0;

            ExceptionCausingEventOnEmissionHandler.IsEnabled = false;

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddInternalEvents();
            serviceCollection.AddLogging();

            serviceCollection.AddTracing(new ConfigurationBuilder().Build()
                                                                   .AsNotNull());

            serviceCollection.ScanAssemblyForImplementations(typeof(EventDispatcherTests).Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider()
                                                   .AsNotNull();

            var internalEventDispatcher = serviceProvider.GetRequiredService<IInternalEventDispatcher>()
                                                         .AsNotNull();

            var tracer = serviceProvider.GetRequiredService<ITracer>()
                                        .AsNotNull();

            tracer.Initialize();

            using (tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            {
                using (var scope = InternalEventsScope.OpenScope())
                {
                    await action(scope, internalEventDispatcher)
                        .AsNotNull();
                }
            }

            tracer.Clear();
        }

        [Fact]
        public async Task DispatchOnEmission()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                await scope.DispatchEventsAsync(dispatcher,
                                                async () =>
                                                {
                                                    await scope.EmitEventAsync(new ExampleEvent());

                                                    Assert.Equal(1, ExampleEventOnEmissionHandler.HandlesCount);
                                                });
            });
        }

        [Fact]
        public void DispatchOnEmissionWithException()
        {
            Assert.ThrowsAsync<Exception>(async () =>
            {
                await TestEventDispatching(async (scope, dispatcher) =>
                {
                    scope      = scope.AsNotNull();
                    dispatcher = dispatcher.AsNotNull();

                    ExceptionCausingEventOnEmissionHandler.IsEnabled = true;

                    await scope.DispatchEventsAsync(dispatcher,
                                                    async () =>
                                                    {
                                                        await scope.EmitEventAsync(new ExceptionCausingEvent());

                                                        Assert.Equal(1, ExceptionCausingEventOnEmissionHandler.HandlesCount);
                                                    });
                });
            });
        }

        [Fact]
        public async Task DispatchOnFailure()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                try
                {
                    await scope.DispatchEventsAsync(dispatcher,
                                                    async () =>
                                                    {
                                                        await scope.EmitEventAsync(new ExampleEvent());

                                                        throw new Exception();
                                                    });
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            });

            Assert.Equal(1, ExampleEventOnFailureHandler.HandlesCount);
        }

        [Fact]
        public async Task DispatchOnFailureWithException()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                try
                {
                    await scope.DispatchEventsAsync(dispatcher,
                                                    async () =>
                                                    {
                                                        await scope.EmitEventAsync(new ExceptionCausingEvent());

                                                        throw new Exception();
                                                    });
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch { }
            });

            Assert.Equal(1, ExceptionCausingEventOnFailureHandler.HandlesCount);
        }

        [Fact]
        public async Task DispatchOnSuccess()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                await scope.DispatchEventsAsync(dispatcher, async () => { await scope.EmitEventAsync(new ExampleEvent()); });
            });

            Assert.Equal(1, ExampleEventOnSuccessHandler.HandlesCount);
        }

        [Fact]
        public async Task DispatchOnSuccessWithException()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                await scope.DispatchEventsAsync(dispatcher, async () => { await scope.EmitEventAsync(new ExceptionCausingEvent()); });
            });

            Assert.Equal(1, ExceptionCausingEventOnSuccessHandler.HandlesCount);
        }
    }
}
