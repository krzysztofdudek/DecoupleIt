using GS.DecoupleIt.Options.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Allows to configure default behaviour of NHibernate extension.
    /// </summary>
    [ConfigureAsNamespace]
    public sealed class Options
    {
        /// <summary>
        ///     Allows to configure transaction behaviour.
        /// </summary>
        [NotNull]
        public TransactionOptions Transaction { get; set; } = new();

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
}
