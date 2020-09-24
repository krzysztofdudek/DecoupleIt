using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using NHibernate;
using NHibernate.Transaction;

#pragma warning disable 618

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Wrapper for <see cref="ITransaction" /> implementing <see cref="IUnitOfWork" />.
    /// </summary>
    [PublicAPI]
    public class NHibernateTransactionUnitOfWorkWrapper : ITransaction, IUnitOfWork
    {
        /// <inheritdoc />
        public bool IsActive => _transactionImplementation.IsActive;

        /// <summary>
        ///     NHibernate session.
        /// </summary>
        [NotNull]
        public ISession Session { get; }

        /// <inheritdoc />
        public bool WasCommitted => _transactionImplementation.WasCommitted;

        /// <inheritdoc />
        public bool WasRolledBack => _transactionImplementation.WasRolledBack;

        /// <inheritdoc />
        public event Action<IUnitOfWork> Disposed;

        /// <summary>
        ///     Creates an instance of <see cref="NHibernateTransactionUnitOfWorkWrapper" />.
        /// </summary>
        /// <param name="unitOfWorkAccessor">Unit of work accessor.</param>
        public NHibernateTransactionUnitOfWorkWrapper([NotNull] IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            Session = unitOfWorkAccessor.Get<NHibernateSessionUnitOfWorkWrapper>();

            _transactionImplementation = Session.BeginTransaction()
                                                .AsNotNull();
        }

        /// <inheritdoc />
        public void Begin()
        {
            _transactionImplementation.Begin();
        }

        /// <inheritdoc />
        public void Begin(IsolationLevel isolationLevel)
        {
            _transactionImplementation.Begin(isolationLevel);
        }

        /// <inheritdoc />
        public void Commit()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            _transactionImplementation.Commit();
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            await _transactionImplementation.CommitAsync(cancellationToken)
                                            .AsNotNull();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(this))
                return;

            _transactionImplementation.Rollback();

            _transactionImplementation.Dispose();

            Session.Dispose();

            Disposed?.Invoke(this);
        }

#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask(Task.CompletedTask);
        }
#endif

        /// <inheritdoc />
        public void Enlist(DbCommand command)
        {
            _transactionImplementation.Enlist(command);
        }

        /// <inheritdoc />
        public void RegisterSynchronization(ISynchronization synchronization)
        {
            _transactionImplementation.RegisterSynchronization(synchronization);
        }

        /// <inheritdoc />
        public void Rollback()
        {
            _transactionImplementation.Rollback();
        }

        /// <inheritdoc />
        public Task RollbackAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _transactionImplementation.RollbackAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void SaveChanges()
        {
            Commit();
        }

        /// <inheritdoc />
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return CommitAsync(cancellationToken)
                .AsNotNull();
        }

        [NotNull]
        private readonly ITransaction _transactionImplementation;
    }
}
