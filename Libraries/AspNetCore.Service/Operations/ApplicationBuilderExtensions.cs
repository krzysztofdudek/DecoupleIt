using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace GS.DecoupleIt.AspNetCore.Service.Operations
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" />.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Uses middleware that dispatches operations that are emitted directly by its own scope.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>Application builder.</returns>
        [NotNull]
        public static IApplicationBuilder UseOperations([NotNull] this IApplicationBuilder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.UseMiddleware<DispatchOperationsMiddleware>();

            return builder;
        }
    }
}
