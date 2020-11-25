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
    public abstract class Optional<T>
    {
        [NotNull]
        public static implicit operator Optional<T>([NotNull] T value)
        {
            return new Some<T>(value);
        }

        [NotNull]
        public static implicit operator Optional<T>(None none)
        {
            return new None<T>();
        }

        /// <summary>
        ///     Does something if optional has value.
        /// </summary>
        /// <param name="doAction">Action.</param>
        public abstract void Do([InstantHandle] [NotNull] DoDelegate doAction);

        /// <summary>
        ///     Does something if optional has value.
        /// </summary>
        /// <param name="doAction">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync([InstantHandle] [NotNull] DoAsyncDelegate doAction, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract Optional<TResult> Map<TResult>([InstantHandle] [NotNull] MapDelegate<TResult> map);

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract Optional<TResult> Map<TResult>([InstantHandle] [NotNull] MapWithNoParamDelegate<TResult> map);

        /// <summary>
        ///     Maps value to another optional.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract Optional<TResult> Map<TResult>([InstantHandle] [NotNull] MapOptionalDelegate<TResult> map);

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Maps value to another optional.
        /// </summary>
        /// <param name="map">Mapping action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([InstantHandle] [NotNull] MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Optionally casts the value to specified type.
        /// </summary>
        /// <typeparam name="TNew">New type.</typeparam>
        /// <returns>Casted value.</returns>
        [NotNull]
        public Optional<TNew> OfType<TNew>()
            where TNew : class
        {
            return this is Some<T> some && typeof(TNew).IsAssignableFrom(typeof(T))
                ? (Optional<TNew>) new Some<TNew>((TNew) (object) some.Content)
                : None.Value;
        }

        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public abstract T Reduce([NotNull] T whenNone);

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public abstract T Reduce([InstantHandle] [NotNull] ReduceDelegate whenNone);

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync([InstantHandle] [NotNull] ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="whenNone">Value given as alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract Optional<T> ReduceToAlternate([NotNull] T whenNone);

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        public abstract Optional<T> ReduceToAlternate([InstantHandle] [NotNull] AlternateDelegate alternateWay);

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public abstract
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync([InstantHandle] [NotNull] AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <returns>Value.</returns>
        public abstract T ReduceToDefault();

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            AlternateAsyncDelegate(CancellationToken cancellationToken);

        [NotNull]
        public delegate Optional<T> AlternateDelegate();

        [NotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsyncDelegate([NotNull] T obj, CancellationToken cancellationToken);

        public delegate void DoDelegate([NotNull] T obj);

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            MapAsyncDelegate<TResult>([NotNull] T obj, CancellationToken cancellationToken);

        [NotNull]
        public delegate TResult MapDelegate<out TResult>([NotNull] T obj);

        [NotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapOptionalAsyncDelegate<TResult>([NotNull] T obj, CancellationToken cancellationToken);

        [NotNull]
        public delegate Optional<TResult> MapOptionalDelegate<TResult>([NotNull] T obj);

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            MapWithNoParamAsyncDelegate<TResult>(CancellationToken cancellationToken);

        [NotNull]
        public delegate TResult MapWithNoParamDelegate<out TResult>();

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsyncDelegate(CancellationToken cancellationToken);

        [NotNull]
        public delegate T ReduceDelegate();
    }
}
