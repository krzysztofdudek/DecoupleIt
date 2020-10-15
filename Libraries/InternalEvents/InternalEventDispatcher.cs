using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.InternalEvents
{
    [Transient]
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

        public Task DispatchOnEmissionAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 true,
                                 cancellationToken);
        }

        public Task DispatchOnFailureAsync(Event @event, Exception exception, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 exception,
                                 false,
                                 cancellationToken);
        }

        public Task DispatchOnSuccessAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return DispatchAsync(@event,
                                 null,
                                 false,
                                 cancellationToken);
        }

        [NotNull]
        private static async Task InvokeEventHandler(
            [NotNull] Event @event,
            [NotNull] object eventHandler,
            [CanBeNull] Exception exception,
            CancellationToken cancellationToken)
        {
            switch (eventHandler)
            {
                case IOnSuccessEventHandler onSuccessEventHandler:
                    await onSuccessEventHandler.HandleAsync(@event, cancellationToken);

                    break;
                case IOnEmissionEventHandler onEmissionEventHandler:
                    await onEmissionEventHandler.HandleAsync(@event, cancellationToken);

                    break;
                case IOnFailureEventHandler onFailureEventHandler when exception != null:
                    await onFailureEventHandler.HandleAsync(@event, exception, cancellationToken);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventHandler), "Event handler is of invalid type.");
            }
        }

        [NotNull]
        private readonly IEventHandlerFactory _eventHandlerFactory;

        [NotNull]
        private readonly ILogger<InternalEventDispatcher> _logger;

        [NotNull]
        private readonly ITracer _tracer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async Task DispatchAsync(
            [NotNull] Event @event,
            [CanBeNull] Exception exception,
            bool onEmission,
            CancellationToken cancellationToken = default)
        {
            using var span = _tracer.OpenChildSpan(@event.GetType(), SpanType.InternalEvent);

            using var internalEventsScope = Scope.InternalEventsScope.OpenScope();

            var eventType = @event.GetType();

            var eventHandlers = onEmission ? _eventHandlerFactory.ResolveOnEmissionEventHandlers(eventType) :
                exception is null          ? _eventHandlerFactory.ResolveOnSuccessEventHandlers(eventType) :
                                             (IReadOnlyCollection<object>) _eventHandlerFactory.ResolveOnFailureEventHandlers(eventType);

            _logger.LogInformation("Event dispatching started, {@EventHandlersCount} will handle it.", eventHandlers.Count);

            try
            {
                await internalEventsScope.DispatchEventsAsync(this,
                                                              () => ProcessEventHandlers(@event,
                                                                                         eventHandlers,
                                                                                         exception,
                                                                                         cancellationToken),
                                                              cancellationToken);

                _logger.LogInformation("Event dispatching finished after {@Duration}ms.", span.Duration.Milliseconds);
            }
            catch
            {
                _logger.LogInformation("Event dispatching failed after {@Duration}ms.", span.Duration.Milliseconds);

                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        private async Task ProcessEventHandler(
            [NotNull] Event @event,
            [CanBeNull] Exception exception,
            [NotNull] object eventHandler,
            CancellationToken cancellationToken)
        {
            using var span = _tracer.OpenChildSpan(eventHandler.GetType(), SpanType.InternalEventHandler);

            using var internalEventsScope = Scope.InternalEventsScope.OpenScope();

            _logger.LogInformation("Event handler invocation started.");

            try
            {
                await internalEventsScope.DispatchEventsAsync(this,
                                                              () => InvokeEventHandler(@event,
                                                                                       eventHandler,
                                                                                       exception,
                                                                                       cancellationToken),
                                                              cancellationToken);

                _logger.LogInformation("Event handler invocation finished after {@Duration}ms.", span.Duration.Milliseconds);
            }
            catch (Exception caughtException)
            {
                _logger.LogInformation(new EventId(),
                                       caughtException,
                                       "Event handler invocation failed after {@Duration}ms.",
                                       span.Duration.Milliseconds);

                if (eventHandler is IOnEmissionEventHandler)
                    throw;
            }
        }

        [NotNull]
        private async Task ProcessEventHandlers(
            [NotNull] Event @event,
            [NotNull] [ItemNotNull] IEnumerable<object> eventHandlers,
            [CanBeNull] Exception exception,
            CancellationToken cancellationToken)
        {
            foreach (var eventHandler in eventHandlers)
                await ProcessEventHandler(@event,
                                          exception,
                                          eventHandler,
                                          cancellationToken);
        }
    }
}
