using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Represents internal events scope, that events are bound to.
    /// </summary>
    [PublicAPI]
    public interface IInternalEventsScope : IDisposable
    {
        /// <summary>
        ///     Events.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ReturnTypeCanBeEnumerable.Global")]
        IReadOnlyCollection<Event> Events { get; }

        /// <summary>
        ///     Event is invoked when event is emitted.
        /// </summary>
        [CanBeNull]
        event EventEmittedAsyncDelegate EventEmitted;

        /// <summary>
        ///     Dispatches events that will be emitted by given scope.
        /// </summary>
        /// <param name="internalEventDispatcher">Internal event dispatcher.</param>
        /// <param name="invokeEvents">Delegate invoked to emit events.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            DispatchEventsAsync(
                [NotNull] IInternalEventDispatcher internalEventDispatcher,
                [NotNull] InvokeEventsAsyncDelegate invokeEvents,
                CancellationToken cancellationToken = default);

        /// <summary>
        ///     Emits an event and attaches it to scope.
        /// </summary>
        /// <param name="event">Event.</param>
        void EmitEvent([NotNull] Event @event);

        /// <summary>
        ///     Emits an event and attaches it to scope.
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
            EmitEventAsync([NotNull] Event @event, CancellationToken cancellationToken = default);
    }
}
