using System;
using System.Threading;
using System.Threading.Tasks;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Base implementation of event handler on emission.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public abstract class OnEmissionEventHandlerBase<TEvent> : IOnEmissionEventHandler<TEvent>
        where TEvent : Event
    {
        /// <inheritdoc />
        public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public Task HandleAsync(Event @event, CancellationToken cancellationToken = default)
        {
            if (@event.GetType() != typeof(TEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleAsync((TEvent) @event, cancellationToken);
        }
    }
}
