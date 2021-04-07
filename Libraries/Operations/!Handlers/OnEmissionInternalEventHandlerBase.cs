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
    ///     Base class for all on emission event handlers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class OnEmissionInternalEventHandlerBase<TEvent> : IOnEmissionInternalEventHandler
        where TEvent : InternalEvent
    {
        /// <summary>
        ///     Handles an event.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected abstract
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync([NotNull] TEvent @event, CancellationToken cancellationToken = default);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IOnEmissionInternalEventHandler.HandleAsync(IInternalEvent @event, CancellationToken cancellationToken)
        {
            if (!(@event is TEvent typedEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleAsync(typedEvent, cancellationToken);
        }
    }
}
