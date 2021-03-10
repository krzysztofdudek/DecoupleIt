using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all commands.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class Command : Operation, ICommand
    {
        /// <summary>
        ///     Dispatches this command.
        /// </summary>
        public void Dispatch()
        {
            OperationDispatcher.DispatchCommandAsync(this, CancellationToken.None)
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                               .AsTask()
#endif
                               .GetAwaiter()
                               .GetResult();
        }

        /// <summary>
        ///     Dispatches this command.
        /// </summary>
        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DispatchAsync(CancellationToken cancellationToken = default)
        {
            return OperationDispatcher.DispatchCommandAsync(this, cancellationToken);
        }

        /// <summary>
        ///     Gets context data of specific <paramref name="key" />.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue GetContextData<TValue>([NotNull] object key)
        {
            if (_executionContext.ContainsKey(key))
                return (TValue) _executionContext[key];

            return default;
        }

        /// <summary>
        ///     Gets context data of specific <typeparamref name="TValue" /> as key.
        /// </summary>
        /// <typeparam name="TValue">Value type and the key at te same time.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue GetContextData<TValue>()
        {
            return GetContextData<TValue>(typeof(TValue));
        }

        /// <summary>
        ///     Gets context data of specific <typeparamref name="TValue" /> as key.
        /// </summary>
        /// <typeparam name="TValue">Value type and the key at te same time.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue? GetContextDataNullable<TValue>()
            where TValue : struct
        {
            if (_executionContext.ContainsKey(typeof(TValue)))
                return (TValue) _executionContext[typeof(TValue)]!;

            return default;
        }

        /// <summary>
        ///     Adds context to this command. It can be used later in post command handlers.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        [NotNull]
        public Command WithContextData([NotNull] object key, [NotNull] object value)
        {
            _executionContext.Add(key, value);

            return this;
        }

        /// <summary>
        ///     Adds context to this command. It can be used later in post command handlers.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <typeparam name="TValue">Type of the value and the key at the same time.</typeparam>
        [NotNull]
        public Command WithContextData<TValue>([NotNull] TValue value)
        {
            return WithContextData(typeof(TValue), value);
        }

        [NotNull]
        private readonly Dictionary<object, object> _executionContext = new();
    }

    /// <summary>
    ///     Base class for all commands.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class Command<TResult> : Operation, ICommandWithResult
    {
        /// <summary>
        ///     Dispatches this command.
        /// </summary>
        /// <returns>Result.</returns>
        [CanBeNull]
        public TResult Dispatch()
        {
            return OperationDispatcher.DispatchCommandAsync(this, CancellationToken.None)
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                                      .AsTask()
#endif
                                      .GetAwaiter()
                                      .GetResult();
        }

        /// <summary>
        ///     Dispatches this command.
        /// </summary>
        /// <returns>Result.</returns>
        [NotNull]
        [ItemCanBeNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            DispatchAsync(CancellationToken cancellationToken = default)
        {
            return OperationDispatcher.DispatchCommandAsync(this, cancellationToken);
        }

        /// <summary>
        ///     Gets context data of specific <paramref name="key" />.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue GetContextData<TValue>([NotNull] object key)
        {
            if (_executionContext.ContainsKey(key))
                return (TValue) _executionContext[key];

            return default;
        }

        /// <summary>
        ///     Gets context data of specific <typeparamref name="TValue" /> as key.
        /// </summary>
        /// <typeparam name="TValue">Value type and the key at te same time.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue GetContextData<TValue>()
        {
            return GetContextData<TValue>(typeof(TValue));
        }

        /// <summary>
        ///     Gets context data of specific <typeparamref name="TValue" /> as key.
        /// </summary>
        /// <typeparam name="TValue">Value type and the key at te same time.</typeparam>
        /// <returns>Value.</returns>
        [CanBeNull]
        public TValue? GetContextDataNullable<TValue>()
            where TValue : struct
        {
            if (_executionContext.ContainsKey(typeof(TValue)))
                return (TValue) _executionContext[typeof(TValue)]!;

            return default;
        }

        /// <summary>
        ///     Adds context to this command. It can be used later in post command handlers.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        [NotNull]
        public Command<TResult> WithContextData([NotNull] object key, [NotNull] object value)
        {
            _executionContext.Add(key, value);

            return this;
        }

        /// <summary>
        ///     Adds context to this command. It can be used later in post command handlers.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <typeparam name="TValue">Type of the value and the key at the same time.</typeparam>
        [NotNull]
        public Command<TResult> WithContextData<TValue>([NotNull] TValue value)
        {
            return WithContextData(typeof(TValue), value);
        }

        [NotNull]
        private readonly Dictionary<object, object> _executionContext = new();

        Type ICommandWithResult.ResultType => typeof(TResult);
    }
}
