using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.NHibernate5
{
    /// <summary>
    ///     Extends <see cref="Builder" />.
    /// </summary>
    [PublicAPI]
    public static class BuilderExtensions
    {
        /// <summary>
        ///     Adds support of NHibernate 5 for unit of work.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder ForNHibernate5([NotNull] this Builder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ServiceCollection.ScanAssemblyForImplementations(ThisAssembly);
            builder.ServiceCollection.ScanAssemblyForOptions(ThisAssembly, builder.Configuration);

            return builder;
        }

        [NotNull]
        private static Assembly ThisAssembly => typeof(BuilderExtensions).Assembly;
    }
}
