using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    [PublicAPI]
    public static class UnitOfWorkAccessorExtensions
    {
        public static void Execute<TUnitOfWork>([NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor, [NotNull] [InstantHandle] Action<TUnitOfWork> action)
            where TUnitOfWork : class, IUnitOfWork
        {
            using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            action(unitOfWork);

            unitOfWork.SaveChanges();
        }

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

        public static async Task ExecuteAsync<TUnitOfWork>(
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

        public static async Task ExecuteAsync<TUnitOfWork>(
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

        public static async Task<TResult> ExecuteAsync<TUnitOfWork, TResult>(
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

        public static async Task<TResult> ExecuteAsync<TUnitOfWork, TResult>(
            [NotNull] this IUnitOfWorkAccessor unitOfWorkAccessor,
            [NotNull] [InstantHandle] Func<TUnitOfWork, TResult> action,
            CancellationToken cancellationToken = default)
            where TUnitOfWork : class, IUnitOfWork
        {
            using var unitOfWork = unitOfWorkAccessor.Get<TUnitOfWork>();

            var result = action(unitOfWork);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}
