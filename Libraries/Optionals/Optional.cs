using System.Threading;
using System.Threading.Tasks;
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
    public readonly struct Optional<T>
    {
        [CanBeNull]
        private readonly Some<T>? _some;

        [CanBeNull]
        private readonly None<T>? _none;

        public Optional(Some<T>? some, None<T>? none)
        {
            _some = some;
            _none = none;
        }

        [NotNull]
        public static implicit operator Optional<T>([NotNull] T value)
        {
            return new Optional<T>(new Some<T>(value), null);
        }

        [NotNull]
        public static implicit operator Optional<T>(Some<T> some)
        {
            return new Optional<T>(some, null);
        }

        [NotNull]
        public static implicit operator Optional<T>(None<T> none)
        {
            return new Optional<T>(null, new None<T>());
        }

        /// <summary>
        ///     Does something if optional has value.
        /// </summary>
        /// <param name="doAction">Action.</param>
        public void Do([InstantHandle] [NotNull] Delegates<T>.DoDelegate doAction)
        {
            _some?.Do(doAction);
            _none?.Do(doAction);
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
            return (_some?.DoAsync(doAction, cancellationToken) ?? _none?.DoAsync(doAction, cancellationToken))!
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                .Value
#endif
                ;
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
            return (_some?.Map(map) ?? _none?.Map(map))!.Value;
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
            return (_some?.Map(map) ?? _none?.Map(map))!.Value;
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
            return (_some?.Map(map) ?? _none?.Map(map))!.Value;
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return (_some?.MapAsync(map, cancellationToken) ?? _none?.MapAsync(map, cancellationToken))!
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                .Value
#endif
                ;
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return (_some?.MapAsync(map, cancellationToken) ?? _none?.MapAsync(map, cancellationToken))!
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                .Value
#endif
                ;
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
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] Delegates<T>.MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return (_some?.MapAsync(map, cancellationToken) ?? _none?.MapAsync(map, cancellationToken))!
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                .Value
#endif
                ;
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
            if (_some.HasValue && typeof(TNew).IsAssignableFrom(typeof(T)))
                return new Optional<TNew>(new Some<TNew>((TNew) (object) _some.Value.Content), null);

            return None<TNew>.Value;
        }

        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [CanBeNull]
        public T Reduce([NotNull] T whenNone)
        {
            return _some.HasValue ? _some.Value.Reduce(whenNone) : _none!.Value.Reduce(whenNone);
        }

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <returns>Value.</returns>
        [CanBeNull]
        public T Reduce([InstantHandle] [NotNull] Delegates<T>.ReduceDelegate whenNone)
        {
            return _some.HasValue ? _some.Value.Reduce(whenNone) : _none!.Value.Reduce(whenNone);
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
            return _some?.ReduceAsync(whenNone, cancellationToken) ?? _none!.Value.ReduceAsync(whenNone, cancellationToken);
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="whenNone">Value given as alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<T> ReduceToAlternate([NotNull] T whenNone)
        {
            return _some?.ReduceToAlternate(whenNone) ?? _none!.Value.ReduceToAlternate(whenNone);
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public Optional<T> ReduceToAlternate([InstantHandle] [NotNull] Delegates<T>.AlternateDelegate alternateWay)
        {
            return _some?.ReduceToAlternate(alternateWay) ?? _none!.Value.ReduceToAlternate(alternateWay);
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
            return _some?.ReduceToAlternateAsync(alternateWay, cancellationToken) ?? _none!.Value.ReduceToAlternateAsync(alternateWay, cancellationToken);
        }

        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <returns>Value.</returns>
        public T ReduceToDefault()
        {
            return _some.HasValue ? _some.Value.ReduceToDefault() : _none!.Value.ReduceToDefault();
        }
    }
}
