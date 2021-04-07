using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore
{
    /// <summary>
    ///     Extends <see cref="Builder" />.
    /// </summary>
    [PublicAPI]
    public static class BuilderExtensions
    {
        /// <summary>
        ///     Adds support of Entity Framework Core for unit of work.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddSupportForEntityFrameworkCore([NotNull] this GS.DecoupleIt.Contextual.UnitOfWork.Builder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ServiceCollection.ScanAssemblyForImplementations(ThisAssembly);
            builder.ServiceCollection.ScanAssemblyForOptions(ThisAssembly, builder.Configuration);

            return new Builder(builder.ServiceCollection, builder.Configuration);
        }

        [NotNull]
        private static Assembly ThisAssembly => typeof(BuilderExtensions).Assembly;
    }
}
