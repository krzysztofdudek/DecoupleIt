using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    public sealed class TransactionOptions
    {
        public bool CleanupSessionOnDisposalOfUncommittedTransaction { get; [UsedImplicitly] set; }
    }
}
