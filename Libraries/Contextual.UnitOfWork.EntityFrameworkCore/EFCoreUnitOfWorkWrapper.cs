using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0
using Microsoft.EntityFrameworkCore.Query;
#endif
#if NETCOREAPP3_1 || NETSTANDARD2_1 || NETSTANDARD2_0
using Microsoft.EntityFrameworkCore.Query.Internal;

#endif

namespace GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore
{
    /// <summary>
    ///     Wrapper for <see cref="DbContext" /> implementing <see cref="IUnitOfWork" />.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "EF1001")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [PublicAPI]
    public class EFCoreUnitOfWorkWrapper
        : IDisposable, IInfrastructure<IServiceProvider>, IDbContextDependencies, IDbSetCache, IDbContextPoolable, IPooledUnitOfWork
#if NET5_0 || NETCOREAPP3_1 || NETSTANDARD2_1
        , IAsyncDisposable
#endif
    {
        [NotNull]
        private DbContext _dbContext;

        /// <summary>
        ///     Creates an instance of <see cref="EFCoreUnitOfWorkWrapper" />.
        /// </summary>
        /// <param name="dbContext">Db context.</param>
        public EFCoreUnitOfWorkWrapper([NotNull] DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            if (!IsPooled)
            {
                _dbContext.Dispose();

                GC.SuppressFinalize(this);
            }

            Disposed?.Invoke(this);
        }

        /// <inheritdoc />
        public IServiceProvider Instance => ((IInfrastructure<IServiceProvider>) _dbContext).Instance;

        /// <inheritdoc />
        public IModel Model => _dbContext.Model;

        /// <inheritdoc />
        public IDbSetSource SetSource => ((IDbContextDependencies) _dbContext).SetSource;

        /// <inheritdoc />
        public IEntityFinderFactory EntityFinderFactory => ((IDbContextDependencies) _dbContext).EntityFinderFactory;

        /// <inheritdoc />
        public IStateManager StateManager => ((IDbContextDependencies) _dbContext).StateManager;

        /// <inheritdoc />
        public IChangeDetector ChangeDetector => ((IDbContextDependencies) _dbContext).ChangeDetector;

        /// <inheritdoc />
        public IEntityGraphAttacher EntityGraphAttacher => ((IDbContextDependencies) _dbContext).EntityGraphAttacher;

        /// <inheritdoc />
        public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger => ((IDbContextDependencies) _dbContext).UpdateLogger;

        /// <inheritdoc />
        public IDiagnosticsLogger<DbLoggerCategory.Infrastructure> InfrastructureLogger => ((IDbContextDependencies) _dbContext).InfrastructureLogger;

        /// <inheritdoc />
        void IResettableService.ResetState()
        {
            ((IResettableService) _dbContext).ResetState();
        }

        public bool IsPooled { get; set; }

        /// <inheritdoc />
        public Task ResetStateAsync(CancellationToken cancellationToken = new())
        {
            return ((IResettableService) _dbContext).ResetStateAsync(cancellationToken);
        }

        /// <inheritdoc />
        public object GetOrAddSet(IDbSetSource source, Type type)
        {
            return ((IDbSetCache) _dbContext).GetOrAddSet(source, type);
        }

        /// <inheritdoc />
        public IAsyncQueryProvider QueryProvider => ((IDbContextDependencies) _dbContext).QueryProvider;

#if NET5_0
        /// <inheritdoc />
        public void SetLease(DbContextLease lease)
        {
            ((IDbContextPoolable) _dbContext).SetLease(lease);
        }

        /// <inheritdoc />
        public void ClearLease()
        {
            ((IDbContextPoolable) _dbContext).ClearLease();
        }

        /// <inheritdoc />
        public void SnapshotConfiguration()
        {
            ((IDbContextPoolable) _dbContext).SnapshotConfiguration();
        }

        /// <inheritdoc />
        public object GetOrAddSet(IDbSetSource source, string entityTypeName, Type type)
        {
            return ((IDbSetCache) _dbContext).GetOrAddSet(source, entityTypeName, type);
        }
#endif

#if NET5_0 || NETCOREAPP3_1 || NETSTANDARD2_1
        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            if (!IsPooled)
            {
                await _dbContext.DisposeAsync();

                GC.SuppressFinalize(this);
            }

            Disposed?.Invoke(this);
        }
#endif

#if NETCOREAPP3_1 || NETSTANDARD2_1 || NETSTANDARD2_0
        /// <inheritdoc />
        public void SetPool(IDbContextPool contextPool)
        {
            ((IDbContextPoolable) _dbContext).SetPool(contextPool);
        }

        /// <inheritdoc />
        public DbContextPoolConfigurationSnapshot SnapshotConfiguration()
        {
            return ((IDbContextPoolable) _dbContext).SnapshotConfiguration();
        }

        /// <inheritdoc />
        public void Resurrect(DbContextPoolConfigurationSnapshot configurationSnapshot)
        {
            ((IDbContextPoolable) _dbContext).Resurrect(configurationSnapshot);
        }
#endif

        public event Action<IUnitOfWork> Disposed;

        /// <inheritdoc />
        public void SaveChanges()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            _dbContext.SaveChanges();
        }

        /// <inheritdoc />
        public
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
#if NETSTANDARD2_0
                return Task.CompletedTask!;
#else
                return new ValueTask();
#endif

            return _dbContext.SaveChangesAsync(cancellationToken)
                             .AsValueTask();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "EF1001")]
        void IPooledUnitOfWork.ResetState()
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1
            ((IDbContextDependencies) _dbContext).StateManager!.ResetState();
#else
            _dbContext.ChangeTracker!.Clear();
#endif
        }
    }
}
