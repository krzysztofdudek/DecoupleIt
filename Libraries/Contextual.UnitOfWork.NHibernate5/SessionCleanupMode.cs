namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Session cleanup mode.
    /// </summary>
    public enum SessionCleanupMode
    {
        /// <summary>
        ///     Session is never cleared automatically.
        /// </summary>
        None,

        /// <summary>
        ///     Session is always cleared after the rollback. All entities are wiped from the session cache.
        /// </summary>
        Clear,

        /// <summary>
        ///     Session is flushed always before the rollback. All entities are marked as non-dirty, but cache contains not current versions of entities.
        /// </summary>
        FlushBeforeRollback
    }
}
