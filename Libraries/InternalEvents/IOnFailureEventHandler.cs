using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Components responsible for handling an event on failure.
    /// </summary>
    [Transient]
    public interface IOnFailureEventHandler
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task HandleAsync([NotNull] Event @event, [NotNull] Exception exception, CancellationToken cancellationToken = default);
    }

    /// <summary>
    ///     Components responsible for handling an event on failure.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    public interface IOnFailureEventHandler<in TEvent> : IOnFailureEventHandler
        where TEvent : Event
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task HandleAsync([NotNull] TEvent @event, [NotNull] Exception exception, CancellationToken cancellationToken = default);
    }
}