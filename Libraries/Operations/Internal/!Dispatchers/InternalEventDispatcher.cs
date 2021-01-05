using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    internal sealed class InternalEventDispatcher : DispatcherBase
    {
        public InternalEventDispatcher(
            [NotNull] IExtendedLoggerFactory extendedLoggerFactory,
            [NotNull] OperationHandlerFactory operationHandlerFactory,
            [NotNull] ITracer tracer) : base(extendedLoggerFactory.Create<InternalEventDispatcher>(), operationHandlerFactory, tracer) { }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnEmissionAsync([NotNull] IInternalEvent @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 true,
                                 cancellationToken);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnFailureAsync([NotNull] IInternalEvent @event, Exception exception, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 exception,
                                 false,
                                 cancellationToken);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnSuccessAsync([NotNull] IInternalEvent @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 false,
                                 cancellationToken);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            InvokeEventHandler(
                [NotNull] IInternalEvent @event,
                [NotNull] object eventHandler,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken)
        {
            return eventHandler switch
            {
                IOnSuccessInternalEventHandler onSuccessEventHandler   => onSuccessEventHandler.HandleAsync(@event, cancellationToken),
                IOnEmissionInternalEventHandler onEmissionEventHandler => onEmissionEventHandler.HandleAsync(@event, cancellationToken),
                IOnFailureInternalEventHandler onFailureEventHandler when exception != null => onFailureEventHandler.HandleAsync(
                    @event,
                    exception,
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(eventHandler), "Event handler is of invalid type.")
            };
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync(
                [NotNull] IInternalEvent @event,
                [CanBeNull] Exception exception,
                bool onEmission,
                CancellationToken cancellationToken = default)
        {
            var eventType = @event.GetType();

            using var span = Tracer.OpenSpan(eventType, SpanType.InternalEvent);

            IReadOnlyCollection<object> eventHandlers;
            string                      mode;

            if (onEmission)
            {
                eventHandlers = OperationHandlerFactory.GetOnEmissionInternalEventHandlers(@event)
                                                       .ToList();

                mode = "on emission";
            }
            else if (exception is null)
            {
                eventHandlers = OperationHandlerFactory.GetOnSuccessInternalEventHandlers(@event)
                                                       .ToList();

                mode = "on success";
            }
            else
            {
                eventHandlers = OperationHandlerFactory.GetOnFailureInternalEventHandlers(@event)
                                                       .ToList();

                mode = "on failure";
            }

            if (eventHandlers.Count == 0)
            {
                Logger.LogInformation("Dispatching event {@EventDispatchingMode} {@OperationAction}, but no handlers found.", mode, "started");

                return;
            }

            Logger.LogInformation("Dispatching event {@EventDispatchingMode} {@OperationAction}, {@OperationHandlersCount} will handle it.",
                                  mode,
                                  "started",
                                  eventHandlers.Count);

            try
            {
                await ProcessEventHandlers(@event,
                                           eventHandlers,
                                           exception,
                                           mode,
                                           cancellationToken);

                Logger.LogInformation("Dispatching event {@EventDispatchingMode} {@OperationAction} after {@OperationDuration}ms.",
                                      mode,
                                      "finished",
                                      span.Duration.Milliseconds);
            }
            catch
            {
                Logger.LogInformation("Dispatching event {@EventDispatchingMode} {@OperationAction} after {@OperationDuration}ms.",
                                      mode,
                                      "failed",
                                      span.Duration.Milliseconds);

                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandler(
                [NotNull] IInternalEvent @event,
                [CanBeNull] Exception exception,
                [NotNull] object eventHandler,
                [NotNull] string mode,
                CancellationToken cancellationToken)
        {
            using var span = Tracer.OpenSpan(eventHandler.GetType(), SpanType.InternalEventHandler);

            Logger.LogInformation("Event handler {@EventDispatchingMode} invocation {@OperationAction}.", mode, "started");

            try
            {
                await InvokeEventHandler(@event,
                                         eventHandler,
                                         exception,
                                         cancellationToken);

                Logger.LogInformation("Event handler {@EventDispatchingMode} invocation {@OperationAction} after {@OperationDuration}ms.",
                                      mode,
                                      "finished",
                                      span.Duration.Milliseconds);
            }
            catch (Exception caughtException)
            {
                Logger.LogInformation(caughtException,
                                      "Event handler {@EventDispatchingMode} invocation {@OperationAction} after {@OperationDuration}ms.",
                                      mode,
                                      "failed",
                                      span.Duration.Milliseconds);

                if (eventHandler is IOnEmissionInternalEventHandler)
                    throw;
            }
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandlers(
                [NotNull] IInternalEvent @event,
                [NotNull] [ItemNotNull] IEnumerable<object> eventHandlers,
                [CanBeNull] Exception exception,
                [NotNull] string mode,
                CancellationToken cancellationToken)
        {
            foreach (var eventHandler in eventHandlers)
                await ProcessEventHandler(@event,
                                          exception,
                                          eventHandler,
                                          mode,
                                          cancellationToken);
        }
    }
}
