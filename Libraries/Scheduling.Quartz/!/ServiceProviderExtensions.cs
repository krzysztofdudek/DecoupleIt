using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GS.DecoupleIt.InternalEvents;
using GS.DecoupleIt.InternalEvents.Scope;
using GS.DecoupleIt.Scheduling.Exceptions;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;

namespace GS.DecoupleIt.Scheduling.Quartz
{
    /// <summary>
    ///     Extends <see cref="IServiceProvider" /> with methods scheduling jobs.
    /// </summary>
    [PublicAPI]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Uses <see cref="Quartz" /> to run all registered jobs.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="configure">Configure builder.</param>
        /// <returns>Service provider.</returns>
        /// <exception cref="NoJobsRegistered">
        ///     Exception is thrown when scheduler is tried to run, but there are no registered
        ///     jobs.
        /// </exception>
        [NotNull]
        public static IServiceProvider UseQuartzScheduling(
            [NotNull] this IServiceProvider serviceProvider,
            [CanBeNull] Action<QuartzSchedulerBuilder> configure = default)
        {
            var builder = new QuartzSchedulerBuilder();

            configure?.Invoke(builder);

            var scheduler = RunScheduler(serviceProvider, builder);

            var jobsCollection = serviceProvider.GetService<IRegisteredJobs>();

            if (jobsCollection is null)
                throw new NoJobsRegistered();

            foreach (var jobEntry in serviceProvider.GetRequiredService<IRegisteredJobs>()
                                                    .AsNotNull())
                ScheduleSingleJob(serviceProvider,
                                  builder,
                                  jobEntry,
                                  scheduler);

            return serviceProvider;
        }

        [UsedImplicitly]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public sealed class BaseQuartzJob : global::Quartz.IJob
        {
            public async Task Execute([NotNull] IJobExecutionContext context)
            {
                if (_jobIsExecuting)
                    return;

                _jobIsExecuting = true;

                var jobParameters = (JobParameters) context.JobDetail.JobDataMap[nameof(JobParameters)]
                                                           .AsNotNull();

                var serviceProvider = jobParameters.ServiceProvider;
                var jobType         = jobParameters.JobType;
                var actionOnError   = jobParameters.ActionOnError;

                var job = (IJob) serviceProvider.GetRequiredService(jobType)
                                                .AsNotNull();

                var internalEventDispatcher = serviceProvider.GetRequiredService<IInternalEventDispatcher>()
                                                             .AsNotNull();

                var tracer = serviceProvider.GetRequiredService<ITracer>()
                                            .AsNotNull();

                var logger = serviceProvider.GetRequiredService<ILogger<BaseQuartzJob>>()
                                            .AsNotNull();

                tracer.Initialize();
                InternalEventsScope.Initialize();

                using (var tracerSpan = tracer.OpenRootSpan(jobType, SpanType.Job))
                {
                    using (var internalEventsScope = InternalEventsScope.OpenScope())
                    {
                        try
                        {
                            logger.LogDebug("Job executing started.");

                            await internalEventsScope.DispatchEventsAsync(internalEventDispatcher,
                                                                          () => job.ExecuteAsync(context.CancellationToken),
                                                                          context.CancellationToken);

                            logger.LogDebug("Job executing finished after {@Duration}.", tracerSpan.Duration.Milliseconds);
                        }
                        catch (Exception exception)
                        {
                            logger.LogDebug("Job execution failed after {@Duration}.", tracerSpan.Duration.Milliseconds);

                            actionOnError?.Invoke(exception, serviceProvider);
                        }
                    }
                }

                InternalEventsScope.Clear();
                tracer.Clear();

                _jobIsExecuting = false;
            }

            private static bool _jobIsExecuting;
        }

        [NotNull]
        private static IScheduler RunScheduler([NotNull] IServiceProvider serviceProvider, [NotNull] QuartzSchedulerBuilder builder)
        {
            builder.SchedulerShutdownToken.Register(async _ =>
                                                    {
                                                        try
                                                        {
                                                            var defaultScheduler =
                                                                await StdSchedulerFactory.GetDefaultScheduler(builder.SchedulerShutdownToken);

                                                            await defaultScheduler.Clear(builder.SchedulerShutdownToken);
                                                        }
                                                        catch (Exception exception)
                                                        {
                                                            serviceProvider.GetRequiredService<ILogger<StdSchedulerFactory>>()
                                                                           .LogError(exception, "StdSchedulerFactory.GetDefaultScheduler() caused a failure.");
                                                        }
                                                    },
                                                    null);

            var scheduler = StdSchedulerFactory.GetDefaultScheduler(builder.SchedulerShutdownToken)
                                               .GetAwaiter()
                                               .GetResult()
                                               .AsNotNull();

            if (!scheduler.IsStarted)
                scheduler.Start(builder.SchedulerShutdownToken)
                         .GetAwaiter()
                         .GetResult();

            return scheduler;
        }

        private static void ScheduleSingleJob(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] QuartzSchedulerBuilder builder,
            JobEntry jobEntry,
            [NotNull] IScheduler scheduler)
        {
            IDictionary<string, object> parameters = new Dictionary<string, object>
            {
                {
                    nameof(JobParameters), new JobParameters(serviceProvider, jobEntry.JobType, builder.ActionOnError)
                }
            };

            var jobBuilder = JobBuilder.Create<BaseQuartzJob>()
                                       .SetJobData(new JobDataMap(parameters));

            var job = jobBuilder.Build();

            var jobTriggerBuilder = TriggerBuilder.Create()
                                                  .WithSimpleSchedule(simpleScheduleBuilder =>
                                                  {
                                                      simpleScheduleBuilder = simpleScheduleBuilder.AsNotNull();

                                                      if (jobEntry.Attribute is SimpleScheduleAttribute attribute)
                                                      {
                                                          simpleScheduleBuilder.WithInterval(new TimeSpan(attribute.Days,
                                                                                                 attribute.Hours,
                                                                                                 attribute.Minutes,
                                                                                                 attribute.Seconds,
                                                                                                 attribute.Milliseconds));

                                                          if (attribute.RepeatCount <= 0)
                                                              simpleScheduleBuilder.RepeatForever();
                                                          else
                                                              simpleScheduleBuilder.WithRepeatCount(attribute.RepeatCount);
                                                      }
                                                  });

            var jobTrigger = jobTriggerBuilder.Build();

            scheduler.ScheduleJob(job, jobTrigger, builder.SchedulerShutdownToken)
                     .GetAwaiter()
                     .GetResult();
        }

        private readonly struct JobParameters
        {
            [NotNull]
            public readonly IServiceProvider ServiceProvider;

            [NotNull]
            public readonly Type JobType;

            [CanBeNull]
            public readonly Action<Exception, IServiceProvider> ActionOnError;

            public JobParameters(
                [NotNull] IServiceProvider serviceProvider,
                [NotNull] Type jobType,
                [CanBeNull] Action<Exception, IServiceProvider> actionOnError)
            {
                ServiceProvider = serviceProvider;
                JobType         = jobType;
                ActionOnError   = actionOnError;
            }
        }
    }
}
