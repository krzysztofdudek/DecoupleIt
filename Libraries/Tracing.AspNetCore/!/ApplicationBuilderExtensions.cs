using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace GS.DecoupleIt.Tracing.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" />.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Uses tracing middleware.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        [NotNull]
        public static IApplicationBuilder UseTracing([NotNull] this IApplicationBuilder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.UseMiddleware<TracingMiddleware>();

            return builder;
        }
    }
}
