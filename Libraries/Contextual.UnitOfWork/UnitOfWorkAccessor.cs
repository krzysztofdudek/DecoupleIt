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

            var entry = GetEntry(unitOfWork.GetType());

            if (entry != null)
            {
                if (entry.Level == 1)
                    return true;

                entry.Level--;
            }

            return false;
        }

        public UnitOfWorkAccessor([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public TUnitOfWork Get<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            var storageEntry = GetEntry(typeof(TUnitOfWork));

            if (storageEntry != null)
            {
                storageEntry.Level++;

                return (TUnitOfWork) storageEntry.UnitOfWork;
            }

            var instance = _serviceProvider.GetRequiredService<TUnitOfWork>()
                                           .AsNotNull();

            lock (StorageEntries)
            {
                (StorageEntries.Value ??= new List<StorageEntry>()).Add(new StorageEntry(instance));
            }

            instance.Disposed += OnInstanceOnDisposed;

            return instance;
        }

        [NotNull]
        private static readonly AsyncLocal<List<StorageEntry>> StorageEntries = new AsyncLocal<List<StorageEntry>>();

        [CanBeNull]
        private static StorageEntry GetEntry([NotNull] Type type)
        {
            lock (StorageEntries)
            {
                return StorageEntries.Value?.SingleOrDefault(x =>
                {
                    var unitOfWorkType = x.AsNotNull()
                                          .UnitOfWork.GetType();

                    return unitOfWorkType == type || type.IsAssignableFrom(unitOfWorkType);
                });
            }
        }

        private static void OnInstanceOnDisposed([NotNull] IUnitOfWork source)
        {
            ContractGuard.IfArgumentIsNull(nameof(source), source);

            StorageEntry entry;

            lock (StorageEntries)
            {
                entry = StorageEntries.Value?.SingleOrDefault(x => ReferenceEquals(x.AsNotNull()
                                                                                    .UnitOfWork,
                                                                                   source));
            }

            if (entry == null)
                return;

            if (entry.Level > 1)
                throw new Exception("Unit of work can be disposed only on the lowest level of usage within async flow.");

            lock (StorageEntries)
            {
                if (StorageEntries.Value == null)
                    throw new Exception("Storage has been cleaned up before disposal.");

                StorageEntries.Value.Remove(entry);

                if (StorageEntries.Value.Count == 0)
                    StorageEntries.Value = null;
            }
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private sealed class StorageEntry
        {
            public long Level = 1;

            [NotNull]
            public readonly IUnitOfWork UnitOfWork;

            public StorageEntry([NotNull] IUnitOfWork unitOfWork)
            {
                UnitOfWork = unitOfWork;
            }
        }
    }
}
