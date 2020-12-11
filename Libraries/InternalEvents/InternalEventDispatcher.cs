using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.InternalEvents
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    internal sealed class InternalEventDispatcher : IInternalEventDispatcher
    {
        public InternalEventDispatcher(
            [NotNull] IEventHandlerFactory eventHandlerFactory,
            [NotNull] ILogger<InternalEventDispatcher> logger,
            [NotNull] ITracer tracer)
        {
            _eventHandlerFactory = eventHandlerFactory;
            _logger              = logger;
            _tracer              = tracer;
        }

        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnEmissionAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 true,
                                 cancellationToken);
        }

        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnFailureAsync(Event @event, Exception exception, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 exception,
                                 false,
                                 cancellationToken);
        }

        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOnSuccessAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 false,
                                 cancellationToken);
        }

        [NotNull]
        private static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            InvokeEventHandler(
                [NotNull] Event @event,
                [NotNull] object eventHandler,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken)
        {
            return eventHandler switch
            {
                IOnSuccessEventHandler onSuccessEventHandler => onSuccessEventHandler.HandleAsync(@event, cancellationToken),
                IOnEmissionEventHandler onEmissionEventHandler => onEmissionEventHandler.HandleAsync(@event, cancellationToken),
                IOnFailureEventHandler onFailureEventHandler when exception != null => onFailureEventHandler.HandleAsync(@event, exception, cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(eventHandler), "Event handler is of invalid type.")
            };
        }

        [NotNull]
        private readonly IEventHandlerFactory _eventHandlerFactory;

        [NotNull]
        private readonly ILogger<InternalEventDispatcher> _logger;

        [NotNull]
        private readonly ITracer _tracer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync(
                [NotNull] Event @event,
                [CanBeNull] Exception exception,
                bool onEmission,
                CancellationToken cancellationToken = default)
        {
            using var span = _tracer.OpenSpan(@event.GetType(), SpanType.InternalEvent);

            using var internalEventsScope = InternalEventsScope.OpenScope();

            var                         eventType = @event.GetType();
            IReadOnlyCollection<object> eventHandlers;
            string                      mode;

            if (onEmission)
            {
                eventHandlers = _eventHandlerFactory.ResolveOnEmissionEventHandlers(eventType)
                                                    .ToList();

                mode = "on emission";
            }
            else if (exception is null)
            {
                eventHandlers = _eventHandlerFactory.ResolveOnSuccessEventHandlers(eventType)
                                                    .ToList();

                mode = "on success";
            }
            else
            {
                eventHandlers = _eventHandlerFactory.ResolveOnFailureEventHandlers(eventType)
                                                    .ToList();

                mode = "on failure";
            }

            if (eventHandlers.Count == 0)
            {
                _logger.LogInformation("Event dispatching {@EventDispatchingMode} started, but no handlers found.", mode);

                return;
            }

            _logger.LogInformation("Event dispatching {@EventDispatchingMode} started, {@EventHandlersCount} will handle it.", mode, eventHandlers.Count);

            try
            {
                await internalEventsScope.DispatchEventsAsync(this,
                                                              () => ProcessEventHandlers(@event,
                                                                                         eventHandlers,
                                                                                         exception,
                                                                                         mode,
                                                                                         cancellationToken),
                                                              cancellationToken);

                _logger.LogInformation("Event dispatching {@EventDispatchingMode} finished after {@Duration}ms.", mode, span.Duration.Milliseconds);
            }
            catch
            {
                _logger.LogInformation("Event dispatching {@EventDispatchingMode} failed after {@Duration}ms.", mode, span.Duration.Milliseconds);

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandler(
                [NotNull] Event @event,
                [CanBeNull] Exception exception,
                [NotNull] object eventHandler,
                [NotNull] string mode,
                CancellationToken cancellationToken)
        {
            using var span = _tracer.OpenSpan(eventHandler.GetType(), SpanType.InternalEventHandler);

            using var internalEventsScope = InternalEventsScope.OpenScope();

            _logger.LogInformation("Event handler {@EventDispatchingMode} invocation started.", mode);

            try
            {
                await internalEventsScope.DispatchEventsAsync(this,
                                                              () => InvokeEventHandler(@event,
                                                                                       eventHandler,
                                                                                       exception,
                                                                                       cancellationToken),
                                                              cancellationToken);

                _logger.LogInformation("Event handler {@EventDispatchingMode} invocation finished after {@Duration}ms.", mode, span.Duration.Milliseconds);
            }
            catch (Exception caughtException)
            {
                _logger.LogInformation(caughtException,
                                       "Event handler {@EventDispatchingMode} invocation failed after {@Duration}ms.",
                                       mode,
                                       span.Duration.Milliseconds);

                if (eventHandler is IOnEmissionEventHandler)
                    throw;
            }
        }

        [NotNull]
        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessEventHandlers(
                [NotNull] Event @event,
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
