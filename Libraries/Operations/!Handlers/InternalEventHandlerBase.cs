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
    ///     Base class for all event handlers.
    /// </summary>
    /// <typeparam name="TEvent">Event type.</typeparam>
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public abstract class InternalEventHandlerBase<TEvent> : IOnEmissionInternalEventHandler, IOnSuccessInternalEventHandler, IOnFailureInternalEventHandler
        where TEvent : InternalEvent
    {
        /// <summary>
        ///     Handles an event on emission.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected virtual
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleOnEmissionAsync([NotNull] TEvent @event, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask;
#else
            return new ValueTask();
#endif
        }

        /// <summary>
        ///     Handles an event on failure.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected virtual
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleOnFailureAsync([NotNull] TEvent @event, [NotNull] Exception exception, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask;
#else
            return new ValueTask();
#endif
        }

        /// <summary>
        ///     Handles an event on success.
        /// </summary>
        /// <param name="event">Event.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        protected virtual
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleOnSuccessAsync([NotNull] TEvent @event, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask;
#else
            return new ValueTask();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IOnEmissionInternalEventHandler.HandleAsync(IInternalEvent @event, CancellationToken cancellationToken)
        {
            if (!(@event is TEvent typedEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleOnEmissionAsync(typedEvent, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IOnSuccessInternalEventHandler.HandleAsync(IInternalEvent @event, CancellationToken cancellationToken)
        {
            if (!(@event is TEvent typedEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleOnSuccessAsync(typedEvent, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IOnFailureInternalEventHandler.HandleAsync(IInternalEvent @event, Exception exception, CancellationToken cancellationToken)
        {
            if (!(@event is TEvent typedEvent))
                throw new ArgumentException("Event is of invalid type.", nameof(@event));

            return HandleOnFailureAsync(typedEvent, exception, cancellationToken);
        }
    }
}
