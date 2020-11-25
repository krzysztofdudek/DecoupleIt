using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Dispatches internal events.
    /// </summary>
    public interface IInternalEventDispatcher
    {
        /// <summary>
        ///     Dispatches event calling it's on emission handlers.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            DispatchOnEmissionAsync([NotNull] Event @event, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Dispatches event calling it's on failure handlers.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            DispatchOnFailureAsync([NotNull] Event @event, [NotNull] Exception exception, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Dispatches event calling it's on success handlers.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            DispatchOnSuccessAsync([NotNull] Event @event, CancellationToken cancellationToken = default);
    }
}
