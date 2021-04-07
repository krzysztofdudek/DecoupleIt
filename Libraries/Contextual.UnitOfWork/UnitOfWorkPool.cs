using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if !NETSTANDARD2_0
using System.Threading.Tasks;

#endif

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    [Singleton]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
    internal sealed class UnitOfWorkPool : IUnitOfWorkPool
    {
        public UnitOfWorkPool([NotNull] IServiceProvider serviceProvider, [NotNull] IOptions<Options> options)
        {
            _serviceProvider = serviceProvider;
            _options         = options.Value!;
        }

        public void Dispose()
        {
            foreach (var item in _pool)
            {
                var poolObject = item.Value.AsNotNull();

                poolObject.MaxPoolSize = 0;

                while (poolObject.Queue.TryDequeue(out var unitOfWork))
                {
                    ((IPooledUnitOfWork) unitOfWork.AsNotNull()).IsPooled = false;

                    unitOfWork!.Dispose();
                }
            }
        }

#if !NETSTANDARD2_0
        public async ValueTask DisposeAsync()
        {
            foreach (var item in _pool)
            {
                var poolObject = item.Value.AsNotNull();

                poolObject.MaxPoolSize = 0;

                while (poolObject.Queue.TryDequeue(out var unitOfWork))
                {
                    ((IPooledUnitOfWork) unitOfWork.AsNotNull()).IsPooled = false;

                    await unitOfWork!.DisposeAsync();
                }
            }
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TUnitOfWork Rent<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            if (!_options.Pooling.Enabled)
                return CreateAnInstanceOfUnitOfWork<TUnitOfWork>();

            var poolObject = GetPoolObject(typeof(TUnitOfWork));

            if (poolObject.Queue.TryDequeue(out var unitOfWork))
            {
                Interlocked.Decrement(ref poolObject.Count);

                return (TUnitOfWork) unitOfWork.AsNotNull();
            }

            var instance = CreateAnInstanceOfUnitOfWork<TUnitOfWork>();

            ((IPooledUnitOfWork) instance).IsPooled = true;

            return instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(IUnitOfWork unitOfWork)
        {
            if (!_options.Pooling.Enabled)
                unitOfWork.Dispose();

            var poolObject = GetPoolObject(unitOfWork.GetType());

            if (Interlocked.Increment(ref poolObject.Count) <= poolObject.MaxPoolSize)
            {
                if (unitOfWork is IPooledUnitOfWork resettableUnitOfWork)
                    resettableUnitOfWork.ResetState();

                poolObject.Queue.Enqueue(unitOfWork);

                return;
            }

            Interlocked.Decrement(ref poolObject.Count);

            ((IPooledUnitOfWork) unitOfWork).IsPooled = false;

            unitOfWork.Dispose();
        }

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly ConcurrentDictionary<Type, PoolObject> _pool = new();

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TUnitOfWork CreateAnInstanceOfUnitOfWork<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            TUnitOfWork instance;
            var         factory = _serviceProvider.GetService<Func<TUnitOfWork>>();

            if (factory is null)
                instance = _serviceProvider.GetRequiredService<TUnitOfWork>()
                                           .AsNotNull();
            else
                instance = factory()
                    .AsNotNull();

            return instance;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PoolObject GetPoolObject([NotNull] Type unitOfWorkType)
        {
            var poolObject = !_pool.ContainsKey(unitOfWorkType)
                ? SetupNewPoolObject(unitOfWorkType)
                : _pool[unitOfWorkType]
                    .AsNotNull();

            return poolObject;
        }

        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PoolObject SetupNewPoolObject([NotNull] Type unitOfWorkType)
        {
            var unitOfWorkName = unitOfWorkType.FullName;

            var defaultOptions = _options.Pooling.Default;

            var dedicatedOptions = unitOfWorkName is not null && _options.Pooling.UnitOfWorks.ContainsKey(unitOfWorkName)
                ? _options.Pooling.UnitOfWorks[unitOfWorkName]
                : null;

            var maxPoolSize = dedicatedOptions?.MaxPoolSize ?? defaultOptions.MaxPoolSize;

            var poolObject = new PoolObject(maxPoolSize);

            if (!_pool.TryAdd(unitOfWorkType, poolObject))
                poolObject = _pool[unitOfWorkType];

            return poolObject!;
        }

        private sealed class PoolObject
        {
            public int Count;
            public int MaxPoolSize;

            [NotNull]
            public readonly ConcurrentQueue<IUnitOfWork> Queue = new();

            public PoolObject(int maxPoolSize)
            {
                MaxPoolSize = maxPoolSize;
            }
        }
    }
}
