using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Allows to configure transaction behaviour.
    /// </summary>
    public sealed class TransactionOptions
    {
        /// <summary>
        ///     Session cleanup mode allows to automatically clean session when rollback of any transaction occurs.
        /// </summary>
        public SessionCleanupMode SessionCleanupMode { get; [UsedImplicitly] set; }
    }
}
