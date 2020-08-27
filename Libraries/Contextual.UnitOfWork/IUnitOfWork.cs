using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Representation of unit of work.
    /// </summary>
    [RegisterManyTimes]
    [PublicAPI]
    public interface IUnitOfWork
        : IDisposable
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
        , IAsyncDisposable
#endif
    {
        /// <summary>
        ///     Event fired when unit of work is disposed.
        /// </summary>
        event Action<IUnitOfWork> Disposed;

        /// <summary>
        ///     Saves changes within all operations done in the context of this unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        ///     Saves changes within all operations done in the context of this unit of work.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task.</returns>
        [NotNull]
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
