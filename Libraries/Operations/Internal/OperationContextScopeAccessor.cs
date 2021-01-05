using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;

namespace GS.DecoupleIt.Operations.Internal
{
    [Singleton]
    internal sealed class OperationContextScopeAccessor : IOperationContextScope
    {
        public IReadOnlyCollection<InternalEvent> InternalEvents => OperationContext.CurrentScope!.InternalEvents;

        public void AggregateEvents(
            IOperationContextScope.AggregateEventsDelegate aggregateEventsMethod,
            IOperationContextScope.ProcessAggregateEventsDelegate processAggregateEventsMethod,
            params Type[] eventTypes)
        {
            OperationContext.CurrentScope?.AggregateEvents(aggregateEventsMethod, processAggregateEventsMethod, eventTypes);
        }

        public Task AggregateEventsAsync(
            IOperationContextScope.AggregateEventsAsyncDelegate aggregateEventsMethod,
            IOperationContextScope.ProcessAggregateEventsAsyncDelegate processAggregateEventsMethod,
            params Type[] eventTypes)
        {
            return OperationContext.CurrentScope?.AggregateEventsAsync(aggregateEventsMethod, processAggregateEventsMethod, eventTypes) ??
                   Task.CompletedTask.AsNotNull();
        }

        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchOperationsAsync(DispatchOperationsDelegate dispatchOperations, CancellationToken cancellationToken = default)
        {
            var task = OperationContext.CurrentScope?.DispatchOperationsAsync(dispatchOperations, cancellationToken);

#if NETCOREAPP2_2 || NETSTANDARD2_0
            return task ?? Task.CompletedTask.AsNotNull();
#else
            return task ?? new ValueTask();
#endif
        }

        public void Dispose()
        {
            OperationContext.CurrentScope?.Dispose();
        }
    }
}
