using System;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.InternalEvents.Scope;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    [TestFixture]
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
            serviceCollection.ScanAssemblyForImplementations(typeof(EventDispatcherTests).Assembly);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var internalEventDispatcher = serviceProvider.GetRequiredService<IInternalEventDispatcher>()
                                                         .AsNotNull();

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(typeof(EventDispatcherTests), SpanType.ExternalRequest))
            {
                using (var scope = InternalEventsScope.OpenScope())
                {
                    await action(scope, internalEventDispatcher)
                        .AsNotNull();
                }
            }

            Tracer.Clear();
        }

        [Test]
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

                                                    Assert.AreEqual(1, ExampleEventOnEmissionHandler.HandlesCount);
                                                });
            });
        }

        [Test]
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

                                                        Assert.AreEqual(1, ExceptionCausingEventOnEmissionHandler.HandlesCount);
                                                    });
                });
            });
        }

        [Test]
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

            Assert.AreEqual(1, ExampleEventOnFailureHandler.HandlesCount);
        }

        [Test]
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

            Assert.AreEqual(1, ExceptionCausingEventOnFailureHandler.HandlesCount);
        }

        [Test]
        public async Task DispatchOnSuccess()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                await scope.DispatchEventsAsync(dispatcher, async () => { await scope.EmitEventAsync(new ExampleEvent()); });
            });

            Assert.AreEqual(1, ExampleEventOnSuccessHandler.HandlesCount);
        }

        [Test]
        public async Task DispatchOnSuccessWithException()
        {
            await TestEventDispatching(async (scope, dispatcher) =>
            {
                scope      = scope.AsNotNull();
                dispatcher = dispatcher.AsNotNull();

                await scope.DispatchEventsAsync(dispatcher, async () => { await scope.EmitEventAsync(new ExceptionCausingEvent()); });
            });

            Assert.AreEqual(1, ExceptionCausingEventOnSuccessHandler.HandlesCount);
        }
    }
}
