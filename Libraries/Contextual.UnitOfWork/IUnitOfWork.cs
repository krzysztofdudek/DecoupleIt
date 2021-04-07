using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Represents unit of work. It contains base methods that are used by <see cref="UnitOfWorkAccessor" />.
    /// </summary>
    [RegisterManyTimes]
    [PublicAPI]
    public interface IUnitOfWork
        : IDisposable
#if !NETSTANDARD2_0
        , IAsyncDisposable
#endif
    {
        /// <summary>
        ///     Event fired when unit of work is disposed.
        /// </summary>
        event Action<IUnitOfWork> Disposed;

        /// <summary>
        ///     Saves all changes made in the context of this unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        ///     Saves all changes made in the context of this unit of work.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
#if NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
