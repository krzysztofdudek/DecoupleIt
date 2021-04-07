namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Represents an unit of work that allows implements pooling.
    /// </summary>
    public interface IPooledUnitOfWork : IUnitOfWork
    {
        /// <summary>
        ///     Indicates if this instance is pooled.
        /// </summary>
        bool IsPooled { set; }

        /// <summary>
        ///     Resets state of unit of work.
        /// </summary>
        void ResetState();
    }
}
