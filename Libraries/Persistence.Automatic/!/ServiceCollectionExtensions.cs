using System.Linq;
using System.Reflection;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GS.DecoupleIt.Persistence.Automatic
{
    public static class ServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection ScanAssemblyForEntities([NotNull] this IServiceCollection serviceCollection, [NotNull] Assembly assembly)
        {
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(new PersistenceContext()));

            var persistenceContext = GetPersistenceContext(serviceCollection);

            var entities = assembly.GetTypes().Where(x => x.GetCustomAttribute<PersistAttribute>() != null).ToList();

            persistenceContext.AddEntities(entities);

            return serviceCollection;
        }

        [NotNull]
        internal static PersistenceContext GetPersistenceContext([NotNull] this IServiceCollection serviceCollection)
        {
            return (PersistenceContext) serviceCollection.Single(x => x?.ServiceType == typeof(PersistenceContext)).AsNotNull().ImplementationInstance.AsNotNull();
        }

        [NotNull]
        public static PersistenceBuilder PersistContext([NotNull] this IServiceCollection serviceCollection, [NotNull] string contextName)
        {
            return new PersistenceBuilder(serviceCollection, contextName);
        }
    }
}
