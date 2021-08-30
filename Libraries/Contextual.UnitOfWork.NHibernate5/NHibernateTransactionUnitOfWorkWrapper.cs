using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
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
        /// <param name="options">Options.</param>
        /// <param name="isolationLevel">Isolation level.</param>
        public NHibernateTransactionUnitOfWorkWrapper(
            [NotNull] IUnitOfWorkAccessor unitOfWorkAccessor,
            [NotNull] IOptions<Options> options,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            Session = unitOfWorkAccessor.Get<NHibernateSessionUnitOfWorkWrapper>();

            var transaction = Session.GetCurrentTransaction();

            if (transaction is not null)
            {
                _transactionImplementation = transaction;

                _externalTransaction = true;
            }
            else
            {
                _transactionImplementation = Session.BeginTransaction(isolationLevel)
                                                    .AsNotNull();
            }

            _options = options.Value!;
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
            if (_externalTransaction)
                return;

            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            _transactionImplementation.Commit();
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_externalTransaction)
                return;

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

            if (!_externalTransaction)
            {
                if (!_transactionImplementation.WasCommitted)
                {
                    if (_options.Transaction.SessionCleanupMode == SessionCleanupMode.FlushBeforeRollback)
                        Session.Flush();

                    _transactionImplementation.Rollback();

                    if (_options.Transaction.SessionCleanupMode == SessionCleanupMode.Clear)
                        Session.Clear();
                }

                _transactionImplementation.Dispose();
            }

            Session.Dispose();

            GC.SuppressFinalize(this);

            Disposed?.Invoke(this);
        }

#if !NETSTANDARD2_0
        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Dispose();

            return new ValueTask();
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
            if (_externalTransaction)
                return;

            _transactionImplementation.Rollback();
        }

        /// <inheritdoc />
        public Task RollbackAsync(CancellationToken cancellationToken = new())
        {
            if (_externalTransaction)
                return Task.CompletedTask;

            return _transactionImplementation.RollbackAsync(cancellationToken);
        }

        /// <inheritdoc />
        public void SaveChanges()
        {
            Commit();
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
            return CommitAsync(cancellationToken)!.AsValueTask();
        }

        private readonly bool _externalTransaction;

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly ITransaction _transactionImplementation;
    }
}
