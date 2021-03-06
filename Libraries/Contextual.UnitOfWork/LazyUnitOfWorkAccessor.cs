using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

#if !NETSTANDARD2_0
using System.Threading.Tasks;

#endif

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    internal sealed class LazyUnitOfWorkAccessor<TUnitOfWork> : ILazyUnitOfWorkAccessor<TUnitOfWork>
        where TUnitOfWork : class, IUnitOfWork
    {
        public bool HasValueLoaded => _value != null;

        public TUnitOfWork Value =>
            _value ??= _factory?.Invoke()
                               .AsNotNull() ?? throw new InvalidOperationException(
                "This should never happen. Both value and factory is null for lazy unit of work accessor.");

        public event Action<ILazyUnitOfWorkAccessor<IUnitOfWork>> Disposed;

        public LazyUnitOfWorkAccessor([NotNull] TUnitOfWork value)
        {
            _value = value;
        }

        public LazyUnitOfWorkAccessor([NotNull] UnitOfWorkFactoryDelegate<TUnitOfWork> factory)
        {
            _factory = factory;
        }

        public void Dispose()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(typeof(TUnitOfWork)))
                return;

            if (_value != null)
                _value.Dispose();
            else
                Disposed?.Invoke(this);
        }

#if !NETSTANDARD2_0
        public ValueTask DisposeAsync()
        {
            if (!UnitOfWorkAccessor.IsLastLevelOfInvocationWithDecrease(typeof(TUnitOfWork)))
                return new ValueTask();

            if (_value != null)
                return _value.DisposeAsync();

            Disposed?.Invoke(this);

            return new ValueTask();
        }
#endif

        [CanBeNull]
        private readonly UnitOfWorkFactoryDelegate<TUnitOfWork> _factory;

        [CanBeNull]
        private TUnitOfWork _value;
    }
}
