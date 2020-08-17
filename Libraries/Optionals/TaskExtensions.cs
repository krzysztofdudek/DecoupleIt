using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    [PublicAPI]
    public static class TaskExtensions
    {
        /// <summary>
        ///     Treats input object as optional.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<T>> AsOptional<T>([CanBeNull] this Task<T> obj)
        {
            return obj is null ? None<T>.Value : (await obj).AsOptional();
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <typeparam name="T">Type of an input optional.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [InstantHandle] [NotNull] Optional<T>.MapAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }

        /// <summary>
        ///     Maps value to another type.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="map">Mapping action.</param>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <typeparam name="T">Type of an input optional.</typeparam>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [InstantHandle] [NotNull] Optional<T>.MapWithNoParamAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }

        /// <summary>
        ///     Maps value to another optional.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="map">Mapping action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of an input optional.</typeparam>
        /// <typeparam name="TResult">Mapping result type.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [InstantHandle] [NotNull] Optional<T>.MapOptionalAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.MapAsync(map, cancellationToken);
        }

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<T> ReduceAsync<T>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [InstantHandle] [NotNull] Optional<T>.ReduceAsyncDelegate whenNone,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.ReduceAsync(whenNone, cancellationToken);
        }

        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<T> ReduceAsync<T>([NotNull] [ItemNotNull] this Task<Optional<T>> task, [NotNull] T whenNone)
        {
            var optional = await task;

            return optional.Reduce(whenNone);
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="alternateWay">Delegate returning alternate value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of an input optional.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<T>> ReduceToAlternateAsync<T>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [InstantHandle] [NotNull] Optional<T>.AlternateAsyncDelegate alternateWay,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.ReduceToAlternateAsync(alternateWay, cancellationToken);
        }

        /// <summary>
        ///     Reduces to an alternate optional.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Value given as alternate value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of an input optional.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async Task<Optional<T>> ReduceToAlternateAsync<T>(
            [NotNull] [ItemNotNull] this Task<Optional<T>> task,
            [NotNull] T whenNone,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return optional.ReduceToAlternate(whenNone);
        }

        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public static async Task<T> ReduceToDefaultAsync<T>([NotNull] [ItemNotNull] this Task<Optional<T>> task)
        {
            var optional = await task;

            return optional.ReduceToDefault();
        }
    }
}
