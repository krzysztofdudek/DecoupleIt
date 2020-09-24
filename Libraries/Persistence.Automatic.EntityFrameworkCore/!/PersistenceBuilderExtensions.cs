using System;
using System.Collections.Generic;
using System.Linq;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GS.DecoupleIt.Persistence.Automatic.EntityFrameworkCore
{
    public static class PersistenceBuilderExtensions
    {
        [NotNull]
        public static IServiceCollection WithEntityFrameworkCore([NotNull] this PersistenceBuilder persistenceBuilder, [CanBeNull] Action<DbContextOptionsBuilder> configureDbContext = default)
        {
            persistenceBuilder.ServiceCollection.TryAdd(ServiceDescriptor.Singleton(new DbContextFactory()));

            var dbContextFactory   = persistenceBuilder.ServiceCollection.GetDbContextFactory();
            var persistenceContext = persistenceBuilder.ServiceCollection.GetPersistenceContext();
            var entitiesTypes      = persistenceContext.GetEntitiesForContext(persistenceBuilder.ContextName);

            var options = new DbContextOptionsBuilder<BaseDbContext>();

            if (configureDbContext != null)
                configureDbContext(options);
            else
                options.UseInMemoryDatabase(persistenceBuilder.ContextName);

            dbContextFactory.AddDbContext(persistenceBuilder.ContextName, options.Options);

            return persistenceBuilder.ServiceCollection;
        }


        [NotNull]
        private static DbContextFactory GetDbContextFactory([NotNull] this IServiceCollection serviceCollection)
        {
            return (DbContextFactory) serviceCollection.Single(x => x?.ServiceType == typeof(DbContextFactory)).AsNotNull().ImplementationInstance.AsNotNull();
        }
    }

    internal sealed class DbContextFactory
    {
        [NotNull]
        private readonly List<(string contextName, DbContextOptions<BaseDbContext> options)> _dbContextsForContextName =
            new List<(string contextName,  DbContextOptions<BaseDbContext> options)>();

        public void AddDbContext([NotNull] string contextName, DbContextOptions<BaseDbContext> options)
        {
            _dbContextsForContextName.Add((contextName, options));
        }
    }

    internal sealed class BaseDbContext : DbContext
    {
        [NotNull]
        private readonly IEnumerable<Type> _entityTypes;

        public BaseDbContext([NotNull] DbContextOptions<BaseDbContext> options, [NotNull] IEnumerable<Type> entityTypes) : base(options)
        {
            _entityTypes = entityTypes;
        }
    }
}
