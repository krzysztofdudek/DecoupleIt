using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Represents pool of unit of work objects.
    /// </summary>
    public interface IUnitOfWorkPool : IDisposable
#if !NETSTANDARD2_0
    , IAsyncDisposable
#endif
    {
        /// <summary>
        ///     Rents an instance of unit of work.
        /// </summary>
        /// <typeparam name="TUnitOfWork">Unit of work type.</typeparam>
        /// <returns>Instance of unit of work.</returns>
        [NotNull]
        public TUnitOfWork Rent<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork;

        /// <summary>
        ///     Returns an unit of work to pool.
        /// </summary>
        /// <param name="pooledUnitOfWork">Instance.</param>
        public void Return([NotNull] IPooledUnitOfWork pooledUnitOfWork);
    }
}
