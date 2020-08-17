using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Allows to retrieve unit of work of specific type from async local storage.
    /// </summary>
    [ProvidesContext]
    public interface IUnitOfWorkAccessor
    {
        /// <summary>
        ///     Gets an instance of unit of work.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Out of scope of unit of work.</exception>
        /// <typeparam name="TUnitOfWork">Unit of work.</typeparam>
        [NotNull]
        TUnitOfWork Get<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork;
    }
}
