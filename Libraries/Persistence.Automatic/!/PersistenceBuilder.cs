using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Persistence.Automatic
{
    public sealed class PersistenceBuilder
    {
        public PersistenceBuilder([NotNull] IServiceCollection serviceCollection, [NotNull] string contextName)
        {
            ServiceCollection = serviceCollection;
            ContextName       = contextName;
        }

        [NotNull]
        public IServiceCollection ServiceCollection { get; }
        
        [NotNull]
        public string ContextName { get; }
    }
}
