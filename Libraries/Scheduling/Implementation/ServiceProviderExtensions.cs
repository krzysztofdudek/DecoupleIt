using System;
using System.Threading;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Scheduling.Implementation
{
    /// <summary>
    ///     Extends <see cref="IServiceProvider" /> with methods scheduling jobs.
    /// </summary>
    [PublicAPI]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Uses default implementation to run all registered jobs.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="cancellationToken">Cancellation token stopping job execution.</param>
        /// <returns>Service provider.</returns>
        [NotNull]
        public static IServiceProvider UseDefaultScheduling([NotNull] this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var jobsCollection = serviceProvider.GetService<IRegisteredJobs>();

            if (jobsCollection is null)
                return serviceProvider;

            var jobExecutor = serviceProvider.GetRequiredService<JobExecutor>()
                                             .AsNotNull();

            jobExecutor.Run(cancellationToken);

            return serviceProvider;
        }
    }
}
