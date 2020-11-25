using GS.DecoupleIt.Scheduling.Implementation;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GS.DecoupleIt.Scheduling.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" /> with methods scheduling jobs.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Uses default implementation to run all registered jobs.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>Application builder.</returns>
        [NotNull]
        public static IApplicationBuilder UseDefaultScheduling([NotNull] this IApplicationBuilder builder)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            var token = builder.ApplicationServices.GetRequiredService<
#if NETCOREAPP2_2
                                   IApplicationLifetime
#else
                                   IHostApplicationLifetime
#endif
                               >()
                               .AsNotNull()
                               .ApplicationStopping;

            builder.ApplicationServices.AsNotNull()
                   .UseDefaultScheduling(token);

            return builder;
        }
    }
}
