using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Operation context scope.
    /// </summary>
    [ProvidesContext]
    [PublicAPI]
    public interface IOperationContextScope : IDisposable
    {
        /// <summary>
        ///     Internal events emitted within this scope.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<InternalEvent> InternalEvents { get; }

        /// <summary>
        ///     Aggregates events and enables to process them in batch.
        /// </summary>
        /// <param name="aggregateEventsMethod">Aggregate events method is supposed to emit events that will be handled.</param>
        /// <param name="processAggregateEventsMethod">Method will be used to process aggregated events.</param>
        /// <param name="eventTypes">Event types to handle.</param>
        void AggregateEvents(
            [NotNull] [InstantHandle] AggregateEventsDelegate aggregateEventsMethod,
            [NotNull] [InstantHandle] ProcessAggregateEventsDelegate processAggregateEventsMethod,
            [NotNull] [ItemNotNull] params Type[] eventTypes);

        /// <summary>
        ///     Aggregates events and enables to process them in batch.
        /// </summary>
        /// <param name="aggregateEventsMethod">Aggregate events method is supposed to emit events that will be handled.</param>
        /// <param name="processAggregateEventsMethod">Method will be used to process aggregated events.</param>
        /// <param name="eventTypes">Event types to handle.</param>
        [NotNull]
        Task AggregateEventsAsync(
            [NotNull] [InstantHandle] AggregateEventsAsyncDelegate aggregateEventsMethod,
            [NotNull] [InstantHandle] ProcessAggregateEventsAsyncDelegate processAggregateEventsMethod,
            [NotNull] [ItemNotNull] params Type[] eventTypes);
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            DispatchOperationsAsync([NotNull] DispatchOperationsDelegate dispatchOperations, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Delegate used for run operations emitting events.
        /// </summary>
        [NotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            AggregateEventsAsyncDelegate();

        /// <summary>
        ///     Delegate used for run operations emitting events.
        /// </summary>
        public delegate void AggregateEventsDelegate();

        /// <summary>
        ///     Delegate used for processing aggregated events.
        /// </summary>
        /// <param name="events">Events.</param>
        /// <returns>Task.</returns>
        [NotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ProcessAggregateEventsAsyncDelegate([NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> events);

        /// <summary>
        ///     Delegate used for processing aggregated events.
        /// </summary>
        /// <param name="events">Events.</param>
        public delegate void ProcessAggregateEventsDelegate([NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> events);
    }
}
