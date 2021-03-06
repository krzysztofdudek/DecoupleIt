using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl;

namespace GS.DecoupleIt.Scheduling.Quartz
{
    /// <summary>
    ///     Extends <see cref="IServiceProvider" /> with methods scheduling jobs.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    public static class ServiceProviderExtensions
    {
        /// <summary>
        ///     Uses <see cref="Quartz" /> to run all registered jobs.
        /// </summary>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="configure">Configure builder delegate.</param>
        /// <returns>Service provider.</returns>
        [NotNull]
        public static IServiceProvider UseQuartzJobScheduling(
            [NotNull] this IServiceProvider serviceProvider,
            [CanBeNull] Action<QuartzSchedulerBuilder> configure = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceProvider), serviceProvider);

            var builder = new QuartzSchedulerBuilder();

            configure?.Invoke(builder);

            var scheduler = RunScheduler(serviceProvider, builder);

            var jobsCollection = serviceProvider.GetService<IRegisteredJobs>();

            if (jobsCollection is null)
                return serviceProvider;

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

                var operationContext = serviceProvider.GetRequiredService<IOperationContext>()
                                                      .AsNotNull();

                var tracer = serviceProvider.GetRequiredService<ITracer>()
                                            .AsNotNull();

                var logger = serviceProvider.GetRequiredService<ILogger<BaseQuartzJob>>()
                                            .AsNotNull();

                var options = serviceProvider.GetRequiredService<IOptions<Options>>()!.Value.AsNotNull();

                using (var tracerSpan = tracer.OpenSpan(jobType, SpanType.Job))
                {
                    using var operationContextScope = operationContext.OpenScope();

                    try
                    {
                        if (options.Logging.EnableNonErrorLogging)
                            logger.LogDebug("Job executing {@OperationAction}.", "started");

                        await operationContextScope.DispatchOperationsAsync(() => job.ExecuteAsync(context.CancellationToken),
                                                                            cancellationToken: context.CancellationToken);

                        if (options.Logging.EnableNonErrorLogging)
                            logger.LogDebug("Job executing {@OperationAction} after {@OperationDuration}ms.", "finished", tracerSpan.Duration.Milliseconds);
                    }
                    catch (Exception exception)
                    {
                        logger.LogError(exception,
                                        "Job execution {@OperationAction} after {@OperationDuration}ms.",
                                        "failed",
                                        tracerSpan.Duration.Milliseconds);

                        actionOnError?.Invoke(exception, serviceProvider);
                    }
                }

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

                                                      if (jobEntry.Attribute is CyclicSchedule attribute)
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
