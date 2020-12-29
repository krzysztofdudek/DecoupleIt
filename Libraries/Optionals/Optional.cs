using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    /// <summary>
    ///     Implements an optional. It can be either something or nothing.
    /// </summary>
    /// <typeparam name="T">Type of an optional value.</typeparam>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CA2012")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
    public readonly struct Optional<T>
    {
        [CanBeNull]
        private readonly T _value;

        private readonly bool _hasValue;

        public Optional([CanBeNull] T value) : this()
        {
            _value    = value;
            _hasValue = value != null;
        }

        [NotNull]
        public static implicit operator Optional<T>([CanBeNull] T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        ///     Does something if optional has value.
        /// </summary>
        /// <param name="doAction">Action.</param>
        public void Do([InstantHandle] [NotNull] Delegates<T>.DoDelegate doAction)
        {
            ContractGuard.IfArgumentIsNull(nameof(doAction), doAction);

            if (_hasValue)
                doAction(_value!);
        }

        /// <summary>
        ///     Does something if optional has value.
        /// </summary>
        /// <param name="doAction">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync([InstantHandle] [NotNull] Delegates<T>.DoAsyncDelegate doAction, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(doAction), doAction);

            if (_hasValue)
                return doAction(_value!, cancellationToken)!;
            else
#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask;
#else
                return new ValueTask();
#endif
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<TResult> Map<TResult>([InstantHandle] [NotNull] Delegates<T>.MapDelegate<TResult> map)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? new Optional<TResult>(map(_value!)) : new Optional<TResult>();
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<TResult> Map<TResult>([InstantHandle] [NotNull] Delegates<T>.MapWithNoParamDelegate<TResult> map)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? new Optional<TResult>(map()) : new Optional<TResult>();
        }

        /// <summary>
        ///     Maps value to another optional.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<TResult> Map<TResult>([InstantHandle] [NotNull] Delegates<T>.MapOptionalDelegate<TResult> map)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? map(_value!) : new Optional<TResult>();
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? new Optional<TResult>(await map(_value!, cancellationToken)!) : new Optional<TResult>();
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? new Optional<TResult>(await map(cancellationToken)!) : new Optional<TResult>();
        }

        /// <summary>
        ///     Maps value to another optional.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(map), map);

            return _hasValue ? await map(_value!, cancellationToken)! : new Optional<TResult>();
        }

        /// <summary>
        ///     Optionally casts the value to specified type.
        /// </summary>
        /// <typeparam name="TNew">New type.</typeparam>
        /// <returns>Casted value.</returns>
        [NotNull]
        public Optional<TNew> OfType<TNew>()
            where TNew : class
        {
            return new Optional<TNew>(_value as TNew);
        }

        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public T Reduce([NotNull] T whenNone)
        {
            ContractGuard.IfArgumentIsNull(nameof(whenNone), whenNone);

            return _value ?? whenNone;
        }

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public T Reduce([InstantHandle] [NotNull] Delegates<T>.ReduceDelegate whenNone)
        {
            ContractGuard.IfArgumentIsNull(nameof(whenNone), whenNone);

            return _value ?? whenNone()
                .AsNotNull();
        }

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync([InstantHandle] [NotNull] Delegates<T>.ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(whenNone), whenNone);

            if (!_hasValue)
                return whenNone(cancellationToken);

#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult(_value);
#else
            return new ValueTask<T>(_value);
#endif
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="whenNone">Value given as alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<T> ReduceToAlternate([NotNull] T whenNone)
        {
            ContractGuard.IfArgumentIsNull(nameof(whenNone), whenNone);

            if (!_hasValue)
                return whenNone;

            return this;
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<T> ReduceToAlternate([InstantHandle] [NotNull] Delegates<T>.AlternateDelegate alternateWay)
        {
            ContractGuard.IfArgumentIsNull(nameof(alternateWay), alternateWay);

            if (!_hasValue)
                return alternateWay();

            return this;
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync([InstantHandle] [NotNull] Delegates<T>.AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(alternateWay), alternateWay);

            if (!_hasValue)
                return alternateWay(cancellationToken);

#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult(this);
#else
            return new ValueTask<Optional<T>>(this);
#endif
        }

        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <returns>Value.</returns>
        public T ReduceToDefault()
        {
            return _value;
        }
    }
}
