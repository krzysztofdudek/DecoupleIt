using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Components responsible for handling an event on emission.
    /// </summary>
    [Singleton]
    [RegisterManyTimes]
    public interface IOnEmissionEventHandler
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task HandleAsync([NotNull] Event @event, CancellationToken cancellationToken = default);
    }

    /// <summary>
    ///     Components responsible for handling an event on emission.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    [RegisterManyTimes]
    [PublicAPI]
    public interface IOnEmissionEventHandler<in TEvent> : IOnEmissionEventHandler
        where TEvent : Event
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task HandleAsync([NotNull] TEvent @event, CancellationToken cancellationToken = default);
    }
}
