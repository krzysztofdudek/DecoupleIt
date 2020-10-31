﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork
{
    /// <inheritdoc />
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
        public static bool IsAvailable<TUnitOfWork>()
            where TUnitOfWork : class, IUnitOfWork
        {
            return GetEntry(typeof(TUnitOfWork)) != null;
        }

        /// <summary>
        ///     Checks if unit of work is on last level of usage.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <returns>Is last level.</returns>
        public static bool IsLastLevelOfInvocation([NotNull] IUnitOfWork unitOfWork)
        {
            ContractGuard.IfArgumentIsNull(nameof(unitOfWork), unitOfWork);

            var entry = GetEntry(unitOfWork.GetType());

            if (entry == null)
                return true;

            return entry.Level == 1;
        }

        /// <summary>
        ///     Checks if unit of work is on last level of usage and decreases level.
        /// </summary>
        /// <param name="unitOfWork">Unit of work.</param>
        /// <returns>Is last level.</returns>
        public static bool IsLastLevelOfInvocationWithDecrease([NotNull] IUnitOfWork unitOfWork)
        {
            ContractGuard.IfArgumentIsNull(nameof(unitOfWork), unitOfWork);

            var entry = GetEntry(unitOfWork.GetType());

            if (entry == null)
                return true;

            if (entry.Level == 1)
                return true;

            entry.Level--;

            return false;
        }

        /// <summary>
        ///     Creates an instance of <see cref="UnitOfWorkAccessor" />.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        public UnitOfWorkAccessor([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
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

            TUnitOfWork instance;
            var         factory = _serviceProvider.GetService<Func<TUnitOfWork>>();

            if (factory is null)
                instance = _serviceProvider.GetRequiredService<TUnitOfWork>()
                                           .AsNotNull();
            else
                instance = factory()
                    .AsNotNull();

            lock (StorageEntries)
            {
                (StorageEntries.Value ??= new List<StorageEntry>()).Add(new StorageEntry(instance));
            }

            instance.Disposed += OnInstanceDisposed;

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

        private static void OnInstanceDisposed([NotNull] IUnitOfWork source)
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

            entry.UnitOfWork.Disposed -= OnInstanceDisposed;
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
