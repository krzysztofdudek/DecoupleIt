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
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            base.Dispose();

            Disposed?.Invoke(this);
        }

#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
        /// <inheritdoc />
        public override ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return new ValueTask();

            return new ValueTask(base.DisposeAsync()
                                     .AsTask()
                                     .AsNotNull()
                                     .ContinueWith(x => Disposed?.Invoke(this)));
        }
#endif

        /// <inheritdoc />
        public new void SaveChanges()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            base.SaveChanges();
        }

        /// <inheritdoc />
        public new Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return Task.CompletedTask;

            return base.SaveChangesAsync(cancellationToken)
                       .AsNotNull();
        }

        /// <inheritdoc />
        protected UnitOfWorkDbContext() { }

        /// <inheritdoc />
        protected UnitOfWorkDbContext([NotNull] DbContextOptions options) : base(options) { }
    }
}
