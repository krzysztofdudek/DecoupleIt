using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Manages internal event scopes.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public sealed class InternalEventsScope : IInternalEventsScope
    {
        /// <summary>
        ///     Aggregates events and enables to process them in batch.
        /// </summary>
        /// <param name="aggregateEventsMethod">Aggregate events method is supposed to emit events that will be handled.</param>
        /// <param name="processAggregateEventsMethod">Method will be used to process aggregated events.</param>
        /// <param name="eventTypes">Event types to handle.</param>
        public static void AggregateEvents(
            [NotNull] [InstantHandle] AggregateEventsDelegate aggregateEventsMethod,
            [NotNull] [InstantHandle] ProcessAggregateEventsDelegate processAggregateEventsMethod,
            [NotNull] [ItemNotNull] params Type[] eventTypes)
        {
            ContractGuard.IfArgumentIsNull(nameof(aggregateEventsMethod), aggregateEventsMethod);
            ContractGuard.IfArgumentIsNull(nameof(processAggregateEventsMethod), processAggregateEventsMethod);
            ContractGuard.IfArgumentNullOrContainsNullItems(nameof(eventTypes), eventTypes);

            var currentStack = Stack;
            var events       = new List<Event>();

#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnEventEmitted(Stack<InternalEventsScope> stack, Event @event, CancellationToken cancellationToken)
            {
                if (currentStack != stack)
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask!.AsValueTask();
#else
                    return new ValueTask();
#endif

                // ReSharper disable once PossibleNullReferenceException
                if (!eventTypes.Contains(@event.GetType()))
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask!.AsValueTask();
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask!.AsValueTask();
#else
                return new ValueTask();
#endif
            }

            StackEventEmitted += OnEventEmitted;

            try
            {
                aggregateEventsMethod();
            }
            finally
            {
                StackEventEmitted -= OnEventEmitted;
            }

            processAggregateEventsMethod(events);
        }

        /// <summary>
        ///     Aggregates events and enables to process them in batch.
        /// </summary>
        /// <param name="aggregateEventsMethod">Aggregate events method is supposed to emit events that will be handled.</param>
        /// <param name="processAggregateEventsMethod">Method will be used to process aggregated events.</param>
        /// <param name="eventTypes">Event types to handle.</param>
        public static async Task AggregateEventsAsync(
            [NotNull] [InstantHandle] AggregateEventsAsyncDelegate aggregateEventsMethod,
            [NotNull] [InstantHandle] ProcessAggregateEventsAsyncDelegate processAggregateEventsMethod,
            [NotNull] [ItemNotNull] params Type[] eventTypes)
        {
            ContractGuard.IfArgumentIsNull(nameof(aggregateEventsMethod), aggregateEventsMethod);
            ContractGuard.IfArgumentIsNull(nameof(processAggregateEventsMethod), processAggregateEventsMethod);
            ContractGuard.IfArgumentNullOrContainsNullItems(nameof(eventTypes), eventTypes);

            var currentStack = Stack;
            var events       = new List<Event>();

#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                OnEventEmitted(Stack<InternalEventsScope> stack, Event @event, CancellationToken cancellationToken)
            {
                if (currentStack != stack)
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask!.AsValueTask();
#else
                    return new ValueTask();
#endif

                // ReSharper disable once PossibleNullReferenceException
                if (!eventTypes.Contains(@event.GetType()))
#if NETCOREAPP2_2 || NETSTANDARD2_0
                    return Task.CompletedTask!.AsValueTask();
#else
                    return new ValueTask();
#endif

                events.Add(@event);

#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask!.AsValueTask();
#else
                return new ValueTask();
#endif
            }

            StackEventEmitted += OnEventEmitted;

            try
            {
                await aggregateEventsMethod()!;
            }
            finally
            {
                StackEventEmitted -= OnEventEmitted;
            }

            await processAggregateEventsMethod(events)!;
        }

        /// <summary>
        ///     Clears async local storage.
        /// </summary>
        public static void Clear()
        {
            if (AsyncLocalStack.Value == null)
                return;

            AsyncLocalStack.Value = null;
        }

        /// <inheritdoc cref="IInternalEventsScope.EmitEvent" />
        internal static void EmitEvent([NotNull] Event @event)
        {
            if (Stack.Count == 0)
                return;

            CurrentScope.EmitEvent(@event);
        }

        /// <inheritdoc cref="IInternalEventsScope.EmitEventAsync" />
        [NotNull]
        internal static
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            EmitEventAsync([NotNull] Event @event, CancellationToken cancellationToken = default)
        {
            return Stack.Count == 0
                ?
#if NETCOREAPP2_2 || NETSTANDARD2_0
                Task.CompletedTask
#else
                new ValueTask()
#endif
                : CurrentScope.EmitEventAsync(@event, cancellationToken);
        }

        /// <summary>
        ///     Initializes async local storage. The best way is to do this on the beginning of thread that will be using internal
        ///     events.
        ///     At the end of usage of internal events is recommended to call <see cref="Clear" /> to clear storage.
        /// </summary>
        public static void Initialize()
        {
            AsyncLocalStack.Value = new Stack<InternalEventsScope>();
        }

        /// <summary>
        ///     Opens new scope.
        /// </summary>
        /// <returns>Internal events scope.</returns>
        [NotNull]
        public static IInternalEventsScope OpenScope()
        {
            var scope = new InternalEventsScope(Stack);

            Stack.Push(scope);

            return scope;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Event> Events => _events;

        public event EventEmittedAsyncDelegate EventEmitted;

        /// <inheritdoc />
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchEventsAsync(
                IInternalEventDispatcher internalEventDispatcher,
                InvokeEventsAsyncDelegate invokeEvents,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(internalEventDispatcher), internalEventDispatcher);
            ContractGuard.IfArgumentIsNull(nameof(invokeEvents), invokeEvents);

#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
                ScopeLifetimeOnEventEmitted(IInternalEventsScope scope, Event @event, CancellationToken cancellationToken2)
            {
                return internalEventDispatcher.DispatchOnEmissionAsync(@event.AsNotNull(), cancellationToken2);
            }

            EventEmitted += ScopeLifetimeOnEventEmitted;

            try
            {
                await invokeEvents()!;

                foreach (var @event in Events)
                    await internalEventDispatcher.DispatchOnSuccessAsync(@event, cancellationToken);
            }
            catch (Exception exception)
            {
                foreach (var @event in Events)
                    await internalEventDispatcher.DispatchOnFailureAsync(@event, exception, cancellationToken);

                throw;
            }
            finally
            {
                EventEmitted -= ScopeLifetimeOnEventEmitted;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed)
                return;

            Stack.Pop();

            if (Stack.Count == 0)
                AsyncLocalStack.Value = null;

            _isDisposed = true;
        }

        [NotNull]
        private static readonly AsyncLocal<Stack<InternalEventsScope>> AsyncLocalStack = new();

        [NotNull]
        public static IInternalEventsScope CurrentScope =>
            Stack.Peek()
                 .AsNotNull();

        [NotNull]
        private static Stack<InternalEventsScope> Stack => AsyncLocalStack.Value ?? (AsyncLocalStack.Value = new Stack<InternalEventsScope>());

        private static event StackEventEmittedAsyncDelegate StackEventEmitted;

        [NotNull]
        [ItemNotNull]
        private readonly List<Event> _events = new();

        private bool _isDisposed;

        [NotNull]
        private readonly Stack<InternalEventsScope> _stack;

        private InternalEventsScope([NotNull] Stack<InternalEventsScope> stackOfEvents)
        {
            _stack = stackOfEvents;
        }

        void IInternalEventsScope.EmitEvent(Event @event)
        {
            ContractGuard.IfArgumentIsNull(nameof(@event), @event);

            _events.Add(@event);

            InvokeEventEmitted(@event, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
            IInternalEventsScope.EmitEventAsync(Event @event, CancellationToken cancellationToken)
        {
            ContractGuard.IfArgumentIsNull(nameof(@event), @event);

            _events.Add(@event);

            return InvokeEventEmitted(@event, cancellationToken);
        }

        private async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task InvokeEventEmitted([NotNull] Event @event, CancellationToken cancellationToken)
        {
            var task = EventEmitted?.Invoke(this, @event, cancellationToken);

            if (task != null)
                await task;

            task = StackEventEmitted?.Invoke(_stack, @event, cancellationToken);

            if (task != null)
                await task;
        }
#else
            ValueTask InvokeEventEmitted([NotNull] Event @event, CancellationToken cancellationToken)
        {
            var task = EventEmitted?.Invoke(this, @event, cancellationToken);

            if (task != null)
                await task.Value;

            task = StackEventEmitted?.Invoke(_stack, @event, cancellationToken);

            if (task != null)
                await task.Value;
        }
#endif


        private delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            StackEventEmittedAsyncDelegate([NotNull] Stack<InternalEventsScope> stack, [NotNull] Event @event, CancellationToken cancellationToken);
    }
}
