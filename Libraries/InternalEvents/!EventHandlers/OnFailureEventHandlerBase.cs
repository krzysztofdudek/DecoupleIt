using System;
using System.Threading;
using System.Threading.Tasks;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Base implementation of event handler on failure.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public abstract class OnFailureEventHandlerBase<TEvent> : IOnFailureEventHandler<TEvent>
        where TEvent : Event
    {
        /// <inheritdoc />
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(TEvent @event, Exception exception, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(Event @event, Exception exception, CancellationToken cancellationToken = default)
        {
            if (@event.GetType() != typeof(TEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleAsync((TEvent) @event, exception, cancellationToken);
        }
    }
}
