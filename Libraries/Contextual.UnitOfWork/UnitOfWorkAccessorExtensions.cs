using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    [PublicAPI]
    public static class UnitOfWorkAccessorExtensions
    {
        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChanges" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        public static void Execute<TUnitOfWork>([NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor, [NotNull] [InstantHandle] Action<TUnitOfWork> action)
            where TUnitOfWork : class, IUnitOfWork
        {
            using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            action(unitOfWork);

            unitOfWork.SaveChanges();
        }

        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChanges" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        /// <typeparam name="TResult">Type of a result.</typeparam>
        public static TResult Execute<TUnitOfWork, TResult>(
            [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
            [NotNull] [InstantHandle] Func<TUnitOfWork, TResult> action)
            where TUnitOfWork : class, IUnitOfWork
        {
            using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            var result = action(unitOfWork);

            unitOfWork.SaveChanges();

            return result;
        }

        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChangesAsync" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ExecuteAsync<TUnitOfWork>(
                [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
                [NotNull] [InstantHandle] Func<TUnitOfWork, Task> action,
                CancellationToken cancellationToken = default)
            where TUnitOfWork : class, IUnitOfWork
        {
#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
            await
#endif
                using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            var task = action(unitOfWork);

            if (task is null)
                throw new MethodDoesNotReturnedTask();

            await task;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChangesAsync" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            ExecuteAsync<TUnitOfWork>(
                [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
                [NotNull] [InstantHandle] Action<TUnitOfWork> action,
                CancellationToken cancellationToken = default)
            where TUnitOfWork : class, IUnitOfWork
        {
#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
            await
#endif
                using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            action(unitOfWork);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChangesAsync" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        /// <typeparam name="TResult">Type of a result.</typeparam>
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            ExecuteAsync<TUnitOfWork, TResult>(
                [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
                [NotNull] [InstantHandle] Func<TUnitOfWork, Task<TResult>> action,
                CancellationToken cancellationToken = default)
            where TUnitOfWork : class, IUnitOfWork
        {
#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
            await
#endif
                using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            var task = action(unitOfWork);

            if (task is null)
                throw new MethodDoesNotReturnedTask();

            var result = await task;

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }

        /// <summary>
        ///     Invokes <paramref name="action" /> with <typeparamref name="TUnitOfWork" /> instance available. After it's invocation,
        ///     <see cref="IUnitOfWork.SaveChangesAsync" /> is called. Method maintains life time of unit of work instance.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        /// <param name="action">Action.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
        /// <typeparam name="TResult">Type of a result.</typeparam>
        public static async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            ExecuteAsync<TUnitOfWork, TResult>(
                [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
                [NotNull] [InstantHandle] Func<TUnitOfWork, TResult> action,
                CancellationToken cancellationToken = default)
            where TUnitOfWork : class, IUnitOfWork
        {
#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
            await
#endif
                using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            var result = action(unitOfWork);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
