using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore
{
    /// <summary>
    ///     DbContext extended with default implementation of <see cref="IUnitOfWork" />.
    /// </summary>
    public abstract class UnitOfWorkDbContext : DbContext, IUnitOfWork
    {
        public event Action<IUnitOfWork> Disposed;

        /// <inheritdoc />
        public override void Dispose()
        {
            if (!UnitOfWorkAccessor.CanBeDisposed(this))
                return;

            base.Dispose();

            Disposed?.Invoke(this);
        }

#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.CanBeDisposed(this))
                return new ValueTask();

            return new ValueTask(base.DisposeAsync()
                                     .AsTask()
                                     .AsNotNull()
                                     .ContinueWith(x => Disposed?.Invoke(this)));
        }
#endif
        /// <inheritdoc />
        protected UnitOfWorkDbContext() { }

        /// <inheritdoc />
        protected UnitOfWorkDbContext([NotNull] DbContextOptions options) : base(options) { }

        /// <inheritdoc />
        void IUnitOfWork.SaveChanges()
        {
            base.SaveChanges();
        }

        /// <inheritdoc />
        Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return base.SaveChangesAsync(cancellationToken)
                       .AsNotNull();
        }
    }
}
