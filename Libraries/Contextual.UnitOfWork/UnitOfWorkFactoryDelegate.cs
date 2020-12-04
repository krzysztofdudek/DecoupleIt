using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    [NotNull]
    internal delegate TUnitOfWork UnitOfWorkFactoryDelegate<out TUnitOfWork>()
        where TUnitOfWork : IUnitOfWork;
}
