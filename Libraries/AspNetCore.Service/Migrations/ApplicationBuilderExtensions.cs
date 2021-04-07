using GS.DecoupleIt.Migrations;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace GS.DecoupleIt.AspNetCore.Service.Migrations
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" />.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Executes migrations that were not executed before.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>Application builder.</returns>
        [NotNull]
        public static IApplicationBuilder ExecuteMigrations([NotNull] this IApplicationBuilder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ApplicationServices.ExecuteMigrations();

            return builder;
        }
    }
}
