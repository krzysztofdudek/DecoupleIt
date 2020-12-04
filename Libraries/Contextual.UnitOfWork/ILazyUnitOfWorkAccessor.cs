using System;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <summary>
    ///     Interface represents lazy loaded factory of unit of work instance.
    /// </summary>
    /// <typeparam name="TUnitOfWork">Type of unit of work.</typeparam>
    public interface ILazyUnitOfWorkAccessor<out TUnitOfWork> : IDisposable
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
    , IAsyncDisposable
#endif
        where TUnitOfWork : class, IUnitOfWork
    {
        /// <summary>
        ///     Unit of work instance.
        /// </summary>
        [NotNull]
        TUnitOfWork Value { get; }

        /// <summary>
        ///     Event fired when instance is disposed.
        /// </summary>
        event Action<ILazyUnitOfWorkAccessor<IUnitOfWork>> Disposed;
    }
}
