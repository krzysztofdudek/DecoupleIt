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

            _transactionImplementation = Session.BeginTransaction(isolationLevel)
                                                .AsNotNull();

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
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocation(this))
                return;

            _transactionImplementation.Commit();
        }

        /// <inheritdoc />
        public async Task CommitAsync(CancellationToken cancellationToken = default)
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

            if (!_transactionImplementation.WasCommitted)
            {
                _transactionImplementation.Rollback();

                if (_options.Transaction.CleanupSessionOnDisposalOfUncommittedTransaction)
                    Session.Clear();
            }

            _transactionImplementation.Dispose();

            Session.Dispose();

            Disposed?.Invoke(this);

            GC.SuppressFinalize(this);
        }

#if !(NETSTANDARD2_0 || NETCOREAPP2_2)
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
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return CommitAsync(cancellationToken)!.AsValueTask();
        }

        protected bool CleanupSessionOnDisposalOfUncommittedTransaction { get; set; }

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly ITransaction _transactionImplementation;
    }
}
