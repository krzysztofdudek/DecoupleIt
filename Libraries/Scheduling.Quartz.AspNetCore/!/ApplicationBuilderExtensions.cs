using System;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GS.DecoupleIt.Scheduling.Quartz.AspNetCore
{
    /// <summary>
    ///     Extends <see cref="IApplicationBuilder" /> with methods scheduling jobs.
    /// </summary>
    [PublicAPI]
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        ///     Uses <see cref="Quartz" /> to run all registered jobs.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <param name="configure">Configure builder.</param>
        /// <returns>Application builder.</returns>
        [NotNull]
        public static IApplicationBuilder UseQuartzSchedulingForAspNetCore(
            [NotNull] this IApplicationBuilder builder,
            [CanBeNull] Action<QuartzSchedulerBuilder> configure = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(builder), builder);

            builder.ApplicationServices.AsNotNull()
                   .UseQuartzScheduling(schedulerBuilder =>
                   {
                       schedulerBuilder = schedulerBuilder.AsNotNull();

                       var token = builder.ApplicationServices.GetRequiredService<
#if NETCOREAPP2_2
                                              IApplicationLifetime
#else
                                              IHostApplicationLifetime
#endif
                                          >()
                                          .AsNotNull()
                                          .ApplicationStopping;

                       schedulerBuilder.WithSchedulerShutdownToken(token);
                   });

            return builder;
        }
    }
}
