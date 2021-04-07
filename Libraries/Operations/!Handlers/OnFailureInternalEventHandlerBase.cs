using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all on failure event handlers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class OnFailureInternalEventHandlerBase<TEvent> : IOnFailureInternalEventHandler
        where TEvent : InternalEvent
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync([NotNull] TEvent @event, [NotNull] Exception exception, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IOnFailureInternalEventHandler.HandleAsync(IInternalEvent @event, Exception exception, CancellationToken cancellationToken)
        {
            if (!(@event is TEvent typedEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleAsync(typedEvent, exception, cancellationToken);
        }
    }
}
