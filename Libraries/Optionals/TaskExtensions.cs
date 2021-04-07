using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
#if !NETSTANDARD2_0
    /// <summary>
    ///     Extends <see cref="Task" /> and <see cref="ValueTask" /> with methods using optionals.
    /// </summary>
#else
    /// <summary>
    ///     Extends <see cref="Task" /> with methods using optionals.
    /// </summary>
#endif
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
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
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            AsOptional<T>([CanBeNull] this Task<T> obj)
        {
            return obj is null ? new Optional<T>() : new Optional<T>(await obj);
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Treats input object as optional.
        /// </summary>
        /// <param name="obj">Input object.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async ValueTask<Optional<T>> AsOptional<T>([CanBeNull] this ValueTask<T> obj)
        {
            return (await obj).AsOptional();
        }
#endif

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
        public static async
#if NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<T, TResult>(
                [NotNull] [ItemNotNull] this Task<Optional<T>> task,
                [InstantHandle] [NotNull] Delegates<T>.MapAsyncDelegate<TResult> map,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }

#if !NETSTANDARD2_0
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
        public static async ValueTask<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [InstantHandle] [NotNull] Delegates<T>.MapAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }
#endif

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
        public static async
#if NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<T, TResult>(
                [NotNull] [ItemNotNull] this Task<Optional<T>> task,
                [InstantHandle] [NotNull] Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }

#if !NETSTANDARD2_0
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
        public static async ValueTask<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [InstantHandle] [NotNull] Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return (await optional.MapAsync(map, cancellationToken)).AsNotNull();
        }
#endif

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
        public static async
#if NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<T, TResult>(
                [NotNull] [ItemNotNull] this Task<Optional<T>> task,
                [InstantHandle] [NotNull] Delegates<T>.MapOptionalAsyncDelegate<TResult> map,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return await optional.MapAsync(map, cancellationToken);
        }

#if !NETSTANDARD2_0
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
        public static async ValueTask<Optional<TResult>> MapAsync<T, TResult>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [InstantHandle] [NotNull] Delegates<T>.MapOptionalAsyncDelegate<TResult> map,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.MapAsync(map, cancellationToken);
        }
#endif

        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async
#if NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync<T>(
                [NotNull] [ItemNotNull] this Task<Optional<T>> task,
                [InstantHandle] [NotNull] Delegates<T>.ReduceAsyncDelegate whenNone,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return await optional.ReduceAsync(whenNone, cancellationToken);
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Reduces value to value returned by the delegate.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Delegate returning value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async ValueTask<T> ReduceAsync<T>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [InstantHandle] [NotNull] Delegates<T>.ReduceAsyncDelegate whenNone,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.ReduceAsync(whenNone, cancellationToken);
        }
#endif

        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async
#if NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync<T>([NotNull] [ItemNotNull] this Task<Optional<T>> task, [NotNull] T whenNone)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return optional.Reduce(whenNone);
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Reduces value to given value.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <param name="whenNone">Value used if optional is none.</param>
        /// <returns>Value.</returns>
        [NotNull]
        [ItemNotNull]
        public static async ValueTask<T> ReduceAsync<T>([NotNull] [ItemNotNull] this ValueTask<Optional<T>> task, [NotNull] T whenNone)
        {
            var optional = await task;

            return optional.Reduce(whenNone);
        }
#endif

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
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync<T>(
                [NotNull] [ItemNotNull] this Task<Optional<T>> task,
                [InstantHandle] [NotNull] Delegates<T>.AlternateAsyncDelegate alternateWay,
                CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return await optional.ReduceToAlternateAsync(alternateWay, cancellationToken);
        }

#if !NETSTANDARD2_0
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
        public static async ValueTask<Optional<T>> ReduceToAlternateAsync<T>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [InstantHandle] [NotNull] Delegates<T>.AlternateAsyncDelegate alternateWay,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return await optional.ReduceToAlternateAsync(alternateWay, cancellationToken);
        }
#endif

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
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync<T>([NotNull] [ItemNotNull] this Task<Optional<T>> task, [NotNull] T whenNone, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return optional.ReduceToAlternate(whenNone);
        }

#if !NETSTANDARD2_0
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
        public static async ValueTask<Optional<T>> ReduceToAlternateAsync<T>(
            [NotNull] [ItemNotNull] this ValueTask<Optional<T>> task,
            [NotNull] T whenNone,
            CancellationToken cancellationToken = default)
        {
            var optional = await task;

            return optional.ReduceToAlternate(whenNone);
        }
#endif

        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public static async
#if NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceToDefaultAsync<T>([NotNull] [ItemNotNull] this Task<Optional<T>> task)
        {
            ContractGuard.IfArgumentIsNull(nameof(task), task);

            var optional = await task;

            return optional.ReduceToDefault();
        }

#if !NETSTANDARD2_0
        /// <summary>
        ///     Reduces value to default.
        /// </summary>
        /// <param name="task">Task.</param>
        /// <returns>Value.</returns>
        [NotNull]
        public static async ValueTask<T> ReduceToDefaultAsync<T>([NotNull] [ItemNotNull] this ValueTask<Optional<T>> task)
        {
            var optional = await task;

            return optional.ReduceToDefault();
        }
#endif
    }
}
