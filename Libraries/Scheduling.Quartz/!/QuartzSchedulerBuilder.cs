using System;
using System.Threading;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Scheduling.Quartz
{
    /// <summary>
    ///     Configures Quartz scheduler.
    /// </summary>
    [PublicAPI]
    public sealed class QuartzSchedulerBuilder
    {
        internal Action<Exception, IServiceProvider> ActionOnError;
        internal CancellationToken SchedulerShutdownToken = CancellationToken.None;

        /// <summary>
        ///     Sets an action called when job fails.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public QuartzSchedulerBuilder WithActionOnError([NotNull] Action<Exception, IServiceProvider> action)
        {
            ActionOnError = action;

            return this;
        }

        /// <summary>
        ///     Sets a cancellation token that stops all jobs and scheduler.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public QuartzSchedulerBuilder WithSchedulerShutdownToken(CancellationToken token)
        {
            SchedulerShutdownToken = token;

            return this;
        }
    }
}
