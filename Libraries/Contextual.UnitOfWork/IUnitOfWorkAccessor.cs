using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Represents an accessor component that stores "unit of works" and allows to retrieve it. It uses async local storage for this purposes, what allows to use
    ///     single unit of work per one async flow, without passing any instance inwards the called code.
    /// </summary>
    [ProvidesContext]
    public interface IUnitOfWorkAccessor
    {
        /// <summary>
        ///     Gets an instance of a unit of work. Type can be the unit of work class itself or any of base classes or implemented
        ///     interfaces. The
        ///     only limitation here, is that type has to be registered in dependency inversion container. For ex. having PermissionsDataContext implementing
        ///     IPermissionsUnitOfWork, allows to resolve both type as the same instance of a class.
        /// </summary>
        /// <typeparam name="TUnitOfWork">
        ///     Type of unit of work, it's base class or one of implemented interfaces.
        /// </typeparam>
        [NotNull]
        TUnitOfWork Get<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork;

        /// <summary>
        ///     Gets an lazy loaded instance of a unit of work. Type can be the unit of work class itself or any of base classes or implemented
        ///     interfaces. The
        ///     only limitation here, is that type has to be registered in dependency inversion container. For ex. having PermissionsDataContext implementing
        ///     IPermissionsUnitOfWork, allows to resolve both type as the same instance of a class.
        /// </summary>
        /// <typeparam name="TUnitOfWork">
        ///     Type of unit of work, it's base class or one of implemented interfaces.
        /// </typeparam>
        [NotNull]
        ILazyUnitOfWorkAccessor<TUnitOfWork> GetLazy<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork;
    }
}
