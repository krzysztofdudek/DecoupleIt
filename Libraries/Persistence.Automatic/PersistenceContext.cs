using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Persistence.Automatic
{
    internal sealed class PersistenceContext
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<EntityEntry> _entityEntries = new List<EntityEntry>();

        public void AddEntities([NotNull] [ItemNotNull] IEnumerable<Type> entityTypes)
        {
            _entityEntries.AddRange(entityTypes.Select(entityType => new EntityEntry(entityType, entityType.GetCustomAttribute<PersistAttribute>() ?? throw new ArgumentException("Collection contains types that are not marked with PersistAttribute."))).ToList());
        }

        [NotNull]
        [ItemNotNull]
        public IEnumerable<Type> GetEntitiesForContext([NotNull] string name)
        {
            return _entityEntries.Where(x => (x.PersistAttribute.ContextName ?? x.EntityType.Assembly.GetName().Name) == name)
                                 .Select(x => x.EntityType);
        }

        private sealed class EntityEntry
        {
            public EntityEntry([NotNull] Type entityType, [NotNull] PersistAttribute persistAttribute)
            {
                EntityType       = entityType;
                PersistAttribute = persistAttribute;
            }

            [NotNull]
            public Type EntityType { get; }

            [NotNull]
            public PersistAttribute PersistAttribute { get; }
        }
    }
}
