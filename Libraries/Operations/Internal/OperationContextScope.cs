using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations.Internal
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    internal sealed class OperationContextScope : IOperationContextScope
    {
        [CanBeNull]
        public OperationContextScope Parent { get; }

        public event OnOperationContextScopeClosedEventHandlerDelegate Closed;

        public event InternalEventEmittedAsyncDelegate InternalEventEmitted;

        public OperationContextScope([NotNull] IServiceProvider serviceProvider, [CanBeNull] OperationContextScope parent = default)
        {
            _serviceProvider = serviceProvider;
            Parent           = parent;
        }

        public void AggregateEvents(
            IOperationContextScope.AggregateEventsDelegate aggregateEventsMethod,
            IOperationContextScope.ProcessAggregateEventsDelegate processAggregateEventsMethod,
            params Type[] eventTypes)
        {
            ContractGuard.IfArgumentIsNull(nameof(aggregateEventsMethod), aggregateEventsMethod);
            ContractGuard.IfArgumentIsNull(nameof(processAggregateEventsMethod), processAggregateEventsMethod);

            var events = new List<InternalEvent>();

#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnInternalEventEmitted(InternalEvent @event, CancellationToken cancellationToken)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (eventTypes.All(x => x != events.GetType()))
#if NETSTANDARD2_0
                    return Task.CompletedTask;
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETSTANDARD2_0
                return Task.CompletedTask;
#else
                return new ValueTask();
#endif
            }

            InternalEventEmitted += OnInternalEventEmitted;

            try
            {
                aggregateEventsMethod();
            }
            finally
            {
                InternalEventEmitted -= OnInternalEventEmitted;
            }

            processAggregateEventsMethod(events);
        }

        public async Task AggregateEventsAsync(
            IOperationContextScope.AggregateEventsAsyncDelegate aggregateEventsMethod,
            IOperationContextScope.ProcessAggregateEventsAsyncDelegate processAggregateEventsMethod,
            params Type[] eventTypes)
        {
            ContractGuard.IfArgumentIsNull(nameof(aggregateEventsMethod), aggregateEventsMethod);
            ContractGuard.IfArgumentIsNull(nameof(processAggregateEventsMethod), processAggregateEventsMethod);

            var events = new List<InternalEvent>();

#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnInternalEventEmitted(InternalEvent @event, CancellationToken cancellationToken)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (eventTypes.All(x => x != events.GetType()))
#if NETSTANDARD2_0
                    return Task.CompletedTask;
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETSTANDARD2_0
                return Task.CompletedTask;
#else
                return new ValueTask();
#endif
            }

            InternalEventEmitted += OnInternalEventEmitted;

            try
            {
                await aggregateEventsMethod()!;
            }
            finally
            {
                InternalEventEmitted -= OnInternalEventEmitted;
            }

            await processAggregateEventsMethod(events)!;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchCommandAsync([NotNull] ICommand command, CancellationToken cancellationToken = default)
        {
            return CommandDispatcher.DispatchAsync(command, cancellationToken);
        }

        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchCommandAsync([NotNull] ICommandWithResult command, CancellationToken cancellationToken = default)
        {
            return CommandDispatcher.DispatchAsync(command, cancellationToken);
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchInternalEventAsync([NotNull] InternalEvent @event, CancellationToken cancellationToken = default)
        {
            var task = InternalEventEmitted?.Invoke(@event, cancellationToken);

#if NETSTANDARD2_0
            return task ?? Task.CompletedTask;
#else
            return task ?? new ValueTask();
#endif
        }

        public async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOperationsAsync(
                DispatchOperationsDelegate dispatchOperations,
                List<InternalEvent> internalEvents = default,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(dispatchOperations), dispatchOperations);

            internalEvents ??= new List<InternalEvent>();

            [NotNull]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnInternalEventEmitted([NotNull] IInternalEvent @event, CancellationToken innerCancellationToken)
            {
                internalEvents.Add((InternalEvent) @event);

                return InternalEventDispatcher.DispatchOnEmissionAsync(@event, innerCancellationToken);
            }

            InternalEventEmitted += OnInternalEventEmitted;

            try
            {
                var task = dispatchOperations();

#if NETSTANDARD2_0
                if (task is null)
                    throw new InvalidOperationException("Dispatch operations delegate returned null instead of task.");
#endif

                await task;

                foreach (var @event in internalEvents)
                    await InternalEventDispatcher.DispatchOnSuccessAsync(@event!, cancellationToken);
            }
            catch (Exception exception)
            {
                foreach (var @event in internalEvents)
                    await InternalEventDispatcher.DispatchOnFailureAsync(@event!, exception, cancellationToken);

                throw;
            }
            finally
            {
                InternalEventEmitted -= OnInternalEventEmitted;
            }
        }

        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETSTANDARD2_0
            Task<object>
#else
            ValueTask<object>
#endif
            DispatchQueryAsync([NotNull] IQuery query, CancellationToken cancellationToken = default)
        {
            return QueryDispatcher.DispatchAsync(query, cancellationToken);
        }

        public void Dispose()
        {
            Closed?.Invoke(this);
        }

        [NotNull]
        [UsedImplicitly]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        [UsedImplicitly]
        private CommandDispatcher CommandDispatcher => _serviceProvider.GetRequiredService<CommandDispatcher>()!;

        [NotNull]
        [UsedImplicitly]
        private InternalEventDispatcher InternalEventDispatcher => _serviceProvider.GetRequiredService<InternalEventDispatcher>()!;

        [NotNull]
        [UsedImplicitly]
        private QueryDispatcher QueryDispatcher => _serviceProvider.GetRequiredService<QueryDispatcher>()!;
    }
}
