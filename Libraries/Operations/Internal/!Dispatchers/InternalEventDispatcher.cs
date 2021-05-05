using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    internal sealed class InternalEventDispatcher : DispatcherBase
    {
        public InternalEventDispatcher(
            [NotNull] IExtendedLoggerFactory extendedLoggerFactory,
            [NotNull] ITracer tracer,
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IOptions<Options> options) : base(extendedLoggerFactory.Create<InternalEventDispatcher>(),
                                                        tracer,
                                                        serviceProvider,
                                                        options) { }

        [NotNull]
        public
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnEmissionAsync([NotNull] IInternalEvent @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 DispatchingMode.OnEmission,
                                 cancellationToken);
        }

        [NotNull]
        public
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnFailureAsync([NotNull] IInternalEvent @event, Exception exception, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 exception,
                                 DispatchingMode.OnFailure,
                                 cancellationToken);
        }

        [NotNull]
        public
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnSuccessAsync([NotNull] IInternalEvent @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 DispatchingMode.OnSuccess,
                                 cancellationToken);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            InvokeEventHandler(
                [NotNull] IInternalEvent @event,
                [NotNull] object eventHandler,
                [CanBeNull] Exception exception,
                DispatchingMode dispatchingMode,
                CancellationToken cancellationToken)
        {
            return eventHandler switch
            {
                IOnSuccessInternalEventHandler onSuccessEventHandler when dispatchingMode == DispatchingMode.OnSuccess => onSuccessEventHandler.HandleAsync(
                    @event,
                    cancellationToken),
                IOnEmissionInternalEventHandler onEmissionEventHandler when dispatchingMode == DispatchingMode.OnEmission => onEmissionEventHandler.HandleAsync(
                    @event,
                    cancellationToken),
                IOnFailureInternalEventHandler onFailureEventHandler when dispatchingMode == DispatchingMode.OnFailure => onFailureEventHandler.HandleAsync(
                    @event,
                    exception!,
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(eventHandler), "Event handler is of invalid type.")
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync(
                [NotNull] IInternalEvent @event,
                [CanBeNull] Exception exception,
                DispatchingMode dispatchingMode,
                CancellationToken cancellationToken = default)
        {
            var eventType = @event.GetType();

            using var span = Tracer.OpenSpan(eventType, SpanType.InternalEvent);

            using var serviceProviderScope = ServiceProvider.CreateScope();

            IEnumerable<object> eventHandlers;
            string              mode;

            switch (dispatchingMode)
            {
                case DispatchingMode.OnEmission:
                    eventHandlers = OperationHandlerFactory.GetOnEmissionInternalEventHandlers(serviceProviderScope!.ServiceProvider!, @event);

                    mode = "on emission";

                    break;
                case DispatchingMode.OnSuccess:
                    eventHandlers = OperationHandlerFactory.GetOnSuccessInternalEventHandlers(serviceProviderScope!.ServiceProvider!, @event);

                    mode = "on success";

                    break;
                case DispatchingMode.OnFailure:
                    eventHandlers = OperationHandlerFactory.GetOnFailureInternalEventHandlers(serviceProviderScope!.ServiceProvider!, @event);

                    mode = "on failure";

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dispatchingMode), dispatchingMode, null);
            }

            if (Options.Logging.EnableNonErrorLogging)
                Logger.LogDebug("Dispatching event {@EventDispatchingMode} {@OperationAction}.", mode, "started");

            try
            {
                await ProcessEventHandlers(@event,
                                           eventHandlers,
                                           exception,
                                           mode,
                                           dispatchingMode,
                                           cancellationToken);

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching event {@EventDispatchingMode} {@OperationAction} after {@OperationDuration}ms.",
                                    mode,
                                    "finished",
                                    span.Duration.Milliseconds);
            }
            catch
            {
                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Dispatching event {@EventDispatchingMode} {@OperationAction} after {@OperationDuration}ms.",
                                    mode,
                                    "failed",
                                    span.Duration.Milliseconds);

                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandler(
                [NotNull] IInternalEvent @event,
                [CanBeNull] Exception exception,
                [NotNull] object eventHandler,
                [NotNull] string mode,
                DispatchingMode dispatchingMode,
                CancellationToken cancellationToken)
        {
            using var span = Tracer.OpenSpan(eventHandler.GetType(), SpanType.InternalEventHandler);

            if (Options.Logging.EnableNonErrorLogging)
                Logger.LogDebug("Event handler {@EventDispatchingMode} invocation {@OperationAction}.", mode, "started");

            try
            {
                await InvokeEventHandler(@event,
                                         eventHandler,
                                         exception,
                                         dispatchingMode,
                                         cancellationToken);

                if (Options.Logging.EnableNonErrorLogging)
                    Logger.LogDebug("Event handler {@EventDispatchingMode} invocation {@OperationAction} after {@OperationDuration}ms.",
                                    mode,
                                    "finished",
                                    span.Duration.Milliseconds);
            }
            catch (Exception caughtException)
            {
                Logger.LogError(caughtException,
                                "Event handler {@EventDispatchingMode} invocation {@OperationAction} after {@OperationDuration}ms.",
                                mode,
                                "failed",
                                span.Duration.Milliseconds);

                if (eventHandler is IOnEmissionInternalEventHandler)
                {
                    caughtException.Data.Add("WasHandled", true);

                    throw;
                }
            }
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandlers(
                [NotNull] IInternalEvent @event,
                [NotNull] [ItemNotNull] IEnumerable<object> eventHandlers,
                [CanBeNull] Exception exception,
                [NotNull] string mode,
                DispatchingMode dispatchingMode,
                CancellationToken cancellationToken)
        {
            foreach (var eventHandler in eventHandlers)
                await ProcessEventHandler(@event,
                                          exception,
                                          eventHandler,
                                          mode,
                                          dispatchingMode,
                                          cancellationToken);
        }

        private enum DispatchingMode
        {
            OnEmission,
            OnSuccess,
            OnFailure
        }
    }
}
