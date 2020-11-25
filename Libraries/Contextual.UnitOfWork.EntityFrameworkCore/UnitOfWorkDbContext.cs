using System;
using System.Threading;
using System.Threading.Tasks;
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

            GC.SuppressFinalize(this);

            Disposed?.Invoke(this);
        }

#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
        /// <inheritdoc />
        public override async ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            await base.DisposeAsync();

            GC.SuppressFinalize(this);

            Disposed?.Invoke(this);
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
        public new
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask!;
#else
                return new ValueTask();
#endif

            return base.SaveChangesAsync(cancellationToken)!.AsValueTask();
        }

        /// <inheritdoc />
        protected UnitOfWorkDbContext() { }

        /// <inheritdoc />
        protected UnitOfWorkDbContext([NotNull] DbContextOptions options) : base(options) { }
    }
}
