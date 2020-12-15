using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Shared
{
    /// <summary>
    ///     Base class for builder classes of extensions.
    /// </summary>
    public abstract class ExtensionBuilderBase
    {
        /// <summary>
        ///     Configuration.
        /// </summary>
        [NotNull]
        public IConfiguration Configuration { get; }

        /// <summary>
        ///     Service collection.
        /// </summary>
        [NotNull]
        public IServiceCollection ServiceCollection { get; }

        /// <summary>
        ///     Creates an instance of <see cref="ExtensionBuilderBase" />.
        /// </summary>
        /// <param name="serviceCollection">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        protected ExtensionBuilderBase([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ServiceCollection = serviceCollection;
            Configuration     = configuration;
        }
    }
}
