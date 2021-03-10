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
        public IReadOnlyCollection<InternalEvent> InternalEvents => _internalEvents;

        [CanBeNull]
        public OperationContextScope Parent { get; }

        public event OnOperationContextScopeClosedEventHandlerDelegate Closed;

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

#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnEventEmitted(InternalEvent @event, CancellationToken cancellationToken)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (eventTypes.All(x => x != events.GetType()))
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask;
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask;
#else
                return new ValueTask();
#endif
            }

            InternalEventEmitted += OnEventEmitted;

            try
            {
                aggregateEventsMethod();
            }
            finally
            {
                InternalEventEmitted -= OnEventEmitted;
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

#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnEventEmitted(InternalEvent @event, CancellationToken cancellationToken)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (eventTypes.All(x => x != events.GetType()))
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask;
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask;
#else
                return new ValueTask();
#endif
            }

            InternalEventEmitted += OnEventEmitted;

            try
            {
                await aggregateEventsMethod()!;
            }
            finally
            {
                InternalEventEmitted -= OnEventEmitted;
            }

            await processAggregateEventsMethod(events)!;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETCOREAPP2_2 || NETSTANDARD2_0
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
#if NETCOREAPP2_2 || NETSTANDARD2_0
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
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchInternalEventAsync([NotNull] InternalEvent @event, CancellationToken cancellationToken = default)
        {
            _internalEvents.Add(@event);

            var task = InternalEventEmitted?.Invoke(@event, cancellationToken);

#if NETCOREAPP2_2 || NETSTANDARD2_0
            return task ?? Task.CompletedTask;
#else
            return task ?? new ValueTask();
#endif
        }

        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOperationsAsync(DispatchOperationsDelegate dispatchOperations, CancellationToken cancellationToken = default)
        {
            InternalEventEmitted += InternalEventDispatcher.DispatchOnEmissionAsync;

            try
            {
                var task = dispatchOperations();

#if NETCOREAPP2_2 || NETSTANDARD2_0
                if (task is null)
                    throw new InvalidOperationException("Dispatch operations delegate returned null instead of task.");
#endif

                await task;

                foreach (var @event in _internalEvents)
                    await InternalEventDispatcher.DispatchOnSuccessAsync(@event, cancellationToken);
            }
            catch (Exception exception)
            {
                foreach (var @event in _internalEvents)
                    await InternalEventDispatcher.DispatchOnFailureAsync(@event, exception, cancellationToken);

                throw;
            }
            finally
            {
                InternalEventEmitted -= InternalEventDispatcher.DispatchOnEmissionAsync;
            }
        }

        [NotNull]
        [ItemCanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal
#if NETCOREAPP2_2 || NETSTANDARD2_0
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
        [ItemNotNull]
        private readonly List<InternalEvent> _internalEvents = new();

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

        private event InternalEventEmittedAsyncDelegate InternalEventEmitted;
    }
}
