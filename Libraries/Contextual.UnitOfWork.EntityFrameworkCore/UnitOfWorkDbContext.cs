using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1
using Microsoft.EntityFrameworkCore.Internal;

#endif

namespace GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore
{
    /// <summary>
    ///     DbContext extended with default implementation of <see cref="IUnitOfWork" />.
    /// </summary>
    public abstract class UnitOfWorkDbContext : DbContext, IPooledUnitOfWork
    {
        public bool IsPooled { get; set; }

        public event Action<IUnitOfWork> Disposed;

        /// <inheritdoc />
        public override void Dispose()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            if (!IsPooled)
            {
                base.Dispose();

                GC.SuppressFinalize(this);
            }

            Disposed?.Invoke(this);
        }

#if !NETSTANDARD2_0
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CA1816")]
        public override async ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            if (!IsPooled)
            {
                await base.DisposeAsync();

                GC.SuppressFinalize(this);
            }

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
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            SaveChangesAsync(CancellationToken cancellationToken)
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
#if NETSTANDARD2_0
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "EF1001")]
        void IPooledUnitOfWork.ResetState()
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1
            ((IDbContextDependencies) this).StateManager!.ResetState();
#else
            ChangeTracker!.Clear();
#endif
        }
    }
}
