using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Scope
{
    /// <summary>
    ///     Manages internal event scopes.
    /// </summary>
    [PublicAPI]
    public sealed class InternalEventsScope : IInternalEventsScope
    {
        [NotNull]
        internal static IInternalEventsScope CurrentScope =>
            Stack.Peek()
                 .AsNotNull();

        /// <summary>
        ///     Event is invoked when event is emitted.
        /// </summary>
        [CanBeNull]
        public static event EventEmittedAsyncDelegate EventEmitted;

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

            Task OnEventEmitted(Stack<InternalEventsScope> stack, Event @event, CancellationToken cancellationToken)
            {
                if (currentStack != stack)
                    return Task.CompletedTask;

                // ReSharper disable once PossibleNullReferenceException
                if (!eventTypes.Contains(@event.GetType()))
                    return Task.CompletedTask;

                events.Add(@event);

                return Task.CompletedTask;
            }

            StackEventEmitted += OnEventEmitted;

            aggregateEventsMethod();

            StackEventEmitted -= OnEventEmitted;

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

            Task OnEventEmitted(Stack<InternalEventsScope> stack, Event @event, CancellationToken cancellationToken)
            {
                if (currentStack != stack)
                    return Task.CompletedTask;

                // ReSharper disable once PossibleNullReferenceException
                if (!eventTypes.Contains(@event.GetType()))
                    return Task.CompletedTask;

                events.Add(@event);

                return Task.CompletedTask;
            }

            StackEventEmitted += OnEventEmitted;

            await aggregateEventsMethod()
                .AsNotNull();

            StackEventEmitted -= OnEventEmitted;

            await processAggregateEventsMethod(events)
                .AsNotNull();
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
            CurrentScope.EmitEvent(@event);
        }

        /// <inheritdoc cref="IInternalEventsScope.EmitEventAsync" />
        [NotNull]
        internal static Task EmitEventAsync([NotNull] Event @event, CancellationToken cancellationToken = default)
        {
            return CurrentScope.EmitEventAsync(@event, cancellationToken);
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

        /// <inheritdoc />
        public async Task DispatchEventsAsync(
            IInternalEventDispatcher internalEventDispatcher,
            InvokeEventsAsyncDelegate invokeEvents,
            CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(internalEventDispatcher), internalEventDispatcher);
            ContractGuard.IfArgumentIsNull(nameof(invokeEvents), invokeEvents);

            async Task ScopeLifetimeOnEventEmitted(IInternalEventsScope scope, Event @event, CancellationToken cancellationToken2)
            {
                await internalEventDispatcher.DispatchOnEmissionAsync(@event.AsNotNull(), cancellationToken2);
            }

            EventEmitted += ScopeLifetimeOnEventEmitted;

            try
            {
                await invokeEvents()
                    .AsNotNull();

                foreach (var @event in Events)
                    await internalEventDispatcher.DispatchOnSuccessAsync(@event, cancellationToken);
            }
            catch (Exception exception)
            {
                foreach (var @event in Events)
                    await internalEventDispatcher.DispatchOnFailureAsync(@event, exception, cancellationToken);

                throw;
            }

            EventEmitted -= ScopeLifetimeOnEventEmitted;
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
        private static readonly AsyncLocal<Stack<InternalEventsScope>> AsyncLocalStack = new AsyncLocal<Stack<InternalEventsScope>>();

        [NotNull]
        private static Stack<InternalEventsScope> Stack => AsyncLocalStack.Value ?? (AsyncLocalStack.Value = new Stack<InternalEventsScope>());

        private static event StackEventEmittedAsyncDelegate StackEventEmitted;

        [NotNull]
        [ItemNotNull]
        private readonly List<Event> _events = new List<Event>();

        private bool _isDisposed;

        [NotNull]
        private readonly Stack<InternalEventsScope> _stack;

        event EventEmittedAsyncDelegate IInternalEventsScope.EventEmitted
        {
            add => InstanceEventEmitted += value;
            remove => InstanceEventEmitted -= value;
        }

        private event EventEmittedAsyncDelegate InstanceEventEmitted;

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

        Task IInternalEventsScope.EmitEventAsync(Event @event, CancellationToken cancellationToken)
        {
            ContractGuard.IfArgumentIsNull(nameof(@event), @event);

            _events.Add(@event);

            return InvokeEventEmitted(@event, cancellationToken);
        }

        private async Task InvokeEventEmitted([NotNull] Event @event, CancellationToken cancellationToken)
        {
            var task = EventEmitted?.Invoke(this, @event, cancellationToken);

            if (task != null)
                await task;

            task = InstanceEventEmitted?.Invoke(this, @event, cancellationToken);

            if (task != null)
                await task;

            task = StackEventEmitted?.Invoke(_stack, @event, cancellationToken);

            if (task != null)
                await task;
        }

        private delegate Task StackEventEmittedAsyncDelegate(
            [NotNull] Stack<InternalEventsScope> stack,
            [NotNull] Event @event,
            CancellationToken cancellationToken);
    }
}
