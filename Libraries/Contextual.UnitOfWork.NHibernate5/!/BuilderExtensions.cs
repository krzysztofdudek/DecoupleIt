using System;
using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
        /// <param name="configure">Configures options.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddSupportForNHibernate5([NotNull] this Builder builder, [CanBeNull] Action<Options> configure = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ServiceCollection.ScanAssemblyForImplementations(ThisAssembly);
            builder.ServiceCollection.ScanAssemblyForOptions(ThisAssembly, builder.Configuration);

            if (configure != null)
                builder.ServiceCollection.PostConfigure(configure);

            return builder;
        }

        [NotNull]
        private static Assembly ThisAssembly => typeof(BuilderExtensions).Assembly;
    }
}
