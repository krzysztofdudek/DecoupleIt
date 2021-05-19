using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <inheritdoc />
    [PublicAPI]
    [Singleton]
    public sealed class UnitOfWorkAccessor : IUnitOfWorkAccessor
    {
        /// <summary>
        ///     Clears storage. Do it after every end of operation processing to ensure that GC can release memory as soon as possible, when unit-of-works are not
        ///     necessary. Also it ensures that mistakenly not disposed unit of works would not leak into the memory.
        /// </summary>
        public static void Clear()
        {
            lock (StorageEntries)
            {
                if (StorageEntries.Value == null)
                    return;

                StorageEntries.Value = null;
            }
        }

        /// <summary>
        ///     Initializes storage. This method should be called at beginning of every operation that will use contextual unit of work. It ensures that storage is empty.
        ///     For ex. this method should be called by middleware before any usage of this class.
        /// </summary>
        public static void Initialize()
        {
            lock (StorageEntries)
            {
                StorageEntries.Value = new List<StorageEntry>();
            }
        }

        /// <summary>
        ///     Checks whether <typeparamref name="TUnitOfWork" /> is currently available in async local storage. Method can be used to validate if unit of work was
        ///     properly disposed.
        /// </summary>
        /// <typeparam name="TUnitOfWork">Type of a unit of work.</typeparam>
        /// <returns>Is available in storage.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAvailable<TUnitOfWork>(out string stackTrace)
            where TUnitOfWork : class, IUnitOfWork
        {
            var entry = GetEntry(typeof(TUnitOfWork));

            if (entry is not null && (entry.UnitOfWork is not null || entry is {LazyUnitOfWorkAccessor: {HasValueLoaded: true}}))
            {
                stackTrace = entry.StackTrace;

                return true;
            }

            stackTrace = null;

            return false;
        }

        /// <summary>
        ///     Checks if unit of work is on last level of usage.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <returns>Is last level.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLastLevelOfInvocation([NotNull] IUnitOfWork unitOfWork)
        {
            ContractGuard.IfArgumentIsNull(nameof(unitOfWork), unitOfWork);

            var entry = GetEntry(unitOfWork.GetType());

            if (entry == null)
                return true;

            return entry.LazyUnitOfWorkAccessor is not null ? entry.Level == 2 : entry.Level == 1;
        }

        /// <summary>
        ///     Checks if unit of work is on last level of usage and decreases level.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <returns>Is last level.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLastLevelOfInvocationWithDecrease([NotNull] IUnitOfWork unitOfWork)
        {
            return IsLastLevelOfInvocationWithDecrease(unitOfWork.GetType());
        }

        /// <summary>
        ///     Checks if unit of work is on last level of usage and decreases level.
        /// </summary>
        /// <param name="unitOfWorkType">Unit of work type.</param>
        /// <returns>Is last level.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLastLevelOfInvocationWithDecrease([NotNull] Type unitOfWorkType)
        {
            ContractGuard.IfArgumentIsNull(nameof(unitOfWorkType), unitOfWorkType);

            var entry = GetEntry(unitOfWorkType);

            if (entry == null)
                return true;

            if (entry.LazyUnitOfWorkAccessor is not null ? entry.Level == 2 : entry.Level == 1)
                return true;

            entry.Level--;

            return false;
        }

        /// <summary>
        ///     Creates an instance of <see cref="UnitOfWorkAccessor" />.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="unitOfWorkPool">Unit of work pool</param>
        /// <param name="options">Options.</param>
        public UnitOfWorkAccessor([NotNull] IServiceProvider serviceProvider, [NotNull] IUnitOfWorkPool unitOfWorkPool, [NotNull] IOptions<Options> options)
        {
            _serviceProvider = serviceProvider;
            _unitOfWorkPool  = unitOfWorkPool;
            _options         = options.Value!;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public TUnitOfWork Get<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            var storageEntry = GetEntry(typeof(TUnitOfWork));

            if (storageEntry != null)
            {
                if (_options.LogStackTrace)
                    storageEntry.StackTrace = Environment.StackTrace;

                if (storageEntry.UnitOfWork is not null)
                {
                    storageEntry.Level++;

                    return (TUnitOfWork) storageEntry.UnitOfWork;
                }

                if (storageEntry.LazyUnitOfWorkAccessor is not null)
                {
                    storageEntry.Level++;

                    return (TUnitOfWork) storageEntry.LazyUnitOfWorkAccessor.Value;
                }

                throw new InvalidOperationException("This should never happen. Unit of work accessor is broken in this thread.");
            }

            var instance = _unitOfWorkPool.Rent<TUnitOfWork>();

            instance.Disposed += OnInstanceDisposed;

            lock (StorageEntries)
            {
                (StorageEntries.Value ??= new List<StorageEntry>()).Add(new StorageEntry(typeof(TUnitOfWork),
                                                                                         instance,
                                                                                         null,
                                                                                         _options.LogStackTrace ? Environment.StackTrace : null));
            }

            return instance;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public ILazyUnitOfWorkAccessor<TUnitOfWork> GetLazy<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            var storageEntry = GetEntry(typeof(TUnitOfWork));

            if (storageEntry != null)
            {
                storageEntry.Level++;

                if (_options.LogStackTrace)
                    storageEntry.StackTrace = Environment.StackTrace;

                return (ILazyUnitOfWorkAccessor<TUnitOfWork>) (storageEntry.LazyUnitOfWorkAccessor ??
                                                               new LazyUnitOfWorkAccessor<IUnitOfWork>(
                                                                   storageEntry.UnitOfWork ??
                                                                   throw new InvalidOperationException(
                                                                       "This should never happen. Unit of work accessor is broken in this thread.")));
            }

            var lazyUnitOfWorkAccessor = new LazyUnitOfWorkAccessor<TUnitOfWork>(() =>
            {
                var instance = _unitOfWorkPool.Rent<TUnitOfWork>();

                instance.Disposed += OnInstanceDisposed;

                return instance;
            });

            lazyUnitOfWorkAccessor.Disposed += OnInstanceDisposed;

            lock (StorageEntries)
            {
                (StorageEntries.Value ??= new List<StorageEntry>()).Add(new StorageEntry(typeof(TUnitOfWork),
                                                                                         null,
                                                                                         lazyUnitOfWorkAccessor,
                                                                                         _options.LogStackTrace ? Environment.StackTrace : null));
            }

            return lazyUnitOfWorkAccessor;
        }

        [NotNull]
        private static readonly AsyncLocal<List<StorageEntry>> StorageEntries = new();

        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static StorageEntry GetEntry([NotNull] Type type)
        {
            lock (StorageEntries)
            {
                return StorageEntries.Value?.SingleOrDefault(x =>
                {
                    var unitOfWorkType = x!.UnitOfWorkType;

                    return unitOfWorkType == type || type.IsAssignableFrom(unitOfWorkType) || unitOfWorkType.IsAssignableFrom(type);
                });
            }
        }

        [NotNull]
        private readonly Options _options;

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        [NotNull]
        private readonly IUnitOfWorkPool _unitOfWorkPool;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearStorageEntry([NotNull] StorageEntry entry)
        {
            if (entry.UnitOfWork != null)
            {
                entry.UnitOfWork.Disposed -= OnInstanceDisposed;

                if (entry.UnitOfWork is IPooledUnitOfWork pooledUnitOfWork)
                    _unitOfWorkPool.Return(pooledUnitOfWork);
            }
            else if (entry.LazyUnitOfWorkAccessor != null)
            {
                entry.LazyUnitOfWorkAccessor.Value.Disposed -= OnInstanceDisposed;

                if (entry.LazyUnitOfWorkAccessor.HasValueLoaded && entry.LazyUnitOfWorkAccessor.Value is IPooledUnitOfWork pooledUnitOfWork)
                    _unitOfWorkPool.Return(pooledUnitOfWork);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InvertIf")]
        private void ManageDisposalOfEntry(StorageEntry entry)
        {
            if (entry == null)
                return;

            if (entry.LazyUnitOfWorkAccessor is not null ? entry.Level > 2 : entry.Level > 1)
                throw new Exception("Unit of work can be disposed only on the lowest level of usage within async flow.");

            lock (StorageEntries)
            {
                if (StorageEntries.Value == null)
                    throw new Exception("Storage has been cleaned up before disposal.");

                StorageEntries.Value.Remove(entry);

                if (StorageEntries.Value.Count == 0)
                    StorageEntries.Value = null;
            }

            ClearStorageEntry(entry);
        }

        private void OnInstanceDisposed([NotNull] IUnitOfWork source)
        {
            ContractGuard.IfArgumentIsNull(nameof(source), source);

            StorageEntry entry;

            lock (StorageEntries)
            {
                entry = StorageEntries.Value?.AsCollectionWithNotNullItems()
                                      .SingleOrDefault(x => ReferenceEquals(
                                                           x.LazyUnitOfWorkAccessor?.HasValueLoaded == true ? x.LazyUnitOfWorkAccessor.Value : x.UnitOfWork,
                                                           source));
            }

            ManageDisposalOfEntry(entry);
        }

        private void OnInstanceDisposed([NotNull] ILazyUnitOfWorkAccessor<IUnitOfWork> source)
        {
            ContractGuard.IfArgumentIsNull(nameof(source), source);

            StorageEntry entry;

            lock (StorageEntries)
            {
                entry = StorageEntries.Value?.SingleOrDefault(x => ReferenceEquals(x.AsNotNull()
                                                                                    .LazyUnitOfWorkAccessor,
                                                                                   source));
            }

            ManageDisposalOfEntry(entry);
        }

        private sealed class StorageEntry
        {
            [CanBeNull]
            public readonly ILazyUnitOfWorkAccessor<IUnitOfWork> LazyUnitOfWorkAccessor;

            public long Level = 1;

            [CanBeNull]
            public string StackTrace;

            [CanBeNull]
            public readonly IUnitOfWork UnitOfWork;

            [NotNull]
            public readonly Type UnitOfWorkType;

            public StorageEntry(
                [NotNull] Type unitOfWorkType,
                [CanBeNull] IUnitOfWork unitOfWork,
                [CanBeNull] ILazyUnitOfWorkAccessor<IUnitOfWork> lazyUnitOfWorkAccessor,
                [CanBeNull] string stackTrace)
            {
                UnitOfWorkType         = unitOfWorkType;
                UnitOfWork             = unitOfWork;
                LazyUnitOfWorkAccessor = lazyUnitOfWorkAccessor;
                StackTrace             = stackTrace;
            }
        }
    }
}
