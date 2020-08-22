using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    [Singleton]
    public sealed class UnitOfWorkAccessor : IUnitOfWorkAccessor
    {
        /// <summary>
        ///     Informs about ability to dispose specific unit of work.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <returns>Can instance be disposed.</returns>
        public static bool CanBeDisposed([NotNull] IUnitOfWork unitOfWork)
        {
            ContractGuard.IfArgumentIsNull(nameof(unitOfWork), unitOfWork);

            var entry = StorageEntries.SingleOrDefault(x => x.Type == unitOfWork.GetType());

            var item = entry?.Storage.Value;

            return item != null && item.Level == 1;
        }

        public UnitOfWorkAccessor([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public TUnitOfWork Get<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            var storageEntry = StorageEntries.SingleOrDefault(x => x.Type == typeof(TUnitOfWork));

            if (storageEntry?.Storage.Value != null)
            {
                storageEntry.Storage.Value.Level++;

                return (TUnitOfWork) storageEntry.Storage.Value.UnitOfWork;
            }

            var instance = _serviceProvider.GetRequiredService<TUnitOfWork>()
                                           .AsNotNull();

            lock (StorageEntries)
            {
                if (storageEntry != null)
                {
                    storageEntry.Storage.Value?.UnitOfWork.Dispose();

                    storageEntry.Storage.Value = new StorageEntry.StorageValue(instance);
                }
                else
                {
                    StorageEntries.Add(new StorageEntry(typeof(TUnitOfWork), instance));
                }
            }

            instance.Disposed += OnInstanceOnDisposed;

            return instance;
        }

        [NotNull]
        [ItemNotNull]
        private static readonly List<StorageEntry> StorageEntries = new List<StorageEntry>();

        private static void OnInstanceOnDisposed([NotNull] IUnitOfWork source)
        {
            ContractGuard.IfArgumentIsNull(nameof(source), source);

            var entry = StorageEntries.Single(x => x.Type == source.GetType())
                                      .AsNotNull();

            var item = entry.Storage.Value;

            if (item == null)
                return;

            if (--item.Level == 0)
                entry.Storage.Value = null;
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private sealed class StorageEntry
        {
            [NotNull]
            public readonly AsyncLocal<StorageValue> Storage = new AsyncLocal<StorageValue>();

            [NotNull]
            public readonly Type Type;

            public StorageEntry([NotNull] Type type, [NotNull] IUnitOfWork unitOfWork)
            {
                Type          = type;
                Storage.Value = new StorageValue(unitOfWork);
            }

            public sealed class StorageValue
            {
                public long Level = 1;

                [NotNull]
                public readonly IUnitOfWork UnitOfWork;

                public StorageValue([NotNull] IUnitOfWork unitOfWork)
                {
                    UnitOfWork = unitOfWork;
                }
            }
        }
    }
}
