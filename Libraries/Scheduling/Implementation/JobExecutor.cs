using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.InternalEvents;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Scheduling.Implementation
{
    [Singleton]
    internal sealed class JobExecutor
    {
        public JobExecutor(
            [NotNull] IRegisteredJobs registeredJobs,
            [NotNull] ILogger<JobExecutor> logger,
            [NotNull] ITracer tracer,
            [NotNull] IInternalEventDispatcher internalEventDispatcher,
            [NotNull] IServiceProvider serviceProvider)
        {
            _registeredJobs          = registeredJobs;
            _logger                  = logger;
            _tracer                  = tracer;
            _internalEventDispatcher = internalEventDispatcher;
            _serviceProvider         = serviceProvider;
        }

        public void Run(CancellationToken cancellationToken = default)
        {
            if (_threads != null)
                return;

            var threads = new List<Thread>();

            foreach (var job in _registeredJobs.Where(x => x.Attribute is SimpleScheduleAttribute))
            {
                var thread = new Thread(() => ExecuteJobThread(job, cancellationToken));

                thread.Start();

                threads.Add(thread);
            }

            _threads = threads;
        }

        [NotNull]
        private readonly IInternalEventDispatcher _internalEventDispatcher;

        [NotNull]
        private readonly ILogger<JobExecutor> _logger;

        [NotNull]
        private readonly IRegisteredJobs _registeredJobs;

        [NotNull]
        private readonly IServiceProvider _serviceProvider;

        private IReadOnlyCollection<Thread> _threads;

        [NotNull]
        private readonly ITracer _tracer;

        private async void ExecuteJobThread(JobEntry jobEntry, CancellationToken cancellationToken)
        {
            var attribute = (SimpleScheduleAttribute) jobEntry.Attribute;

            long iteration     = 0;
            var  isForeverLoop = attribute.RepeatCount <= 0;

            var sleepTimeFromAttribute = new TimeSpan(attribute.Days,
                                                      attribute.Hours,
                                                      attribute.Minutes,
                                                      attribute.Seconds,
                                                      attribute.Milliseconds);

            var lastIterationDuration = TimeSpan.Zero;

            while (true)
            {
                if (!isForeverLoop)
                {
                    if (iteration < attribute.RepeatCount)
                        iteration++;
                    else
                        return;
                }

                if (cancellationToken.IsCancellationRequested)
                    return;

                try
                {
                    _tracer.Initialize();
                    InternalEventsScope.Initialize();

                    using (var tracerSpan = _tracer.OpenRootSpan(jobEntry.JobType.FullName!, SpanType.Job))
                    {
                        using (var internalEventsScope = InternalEventsScope.OpenScope())
                        {
                            try
                            {
                                _logger.LogDebug("Job executing started.");

                                var job = (IJob) _serviceProvider.GetRequiredService(jobEntry.JobType)
                                                                 .AsNotNull();

                                await internalEventsScope.DispatchEventsAsync(_internalEventDispatcher,
                                                                              () => job.ExecuteAsync(cancellationToken),
                                                                              cancellationToken);

                                lastIterationDuration = tracerSpan.Duration;

                                _logger.LogDebug("Job executing finished after {@Duration}ms.", lastIterationDuration.Milliseconds);
                            }
                            catch (Exception exception)
                            {
                                lastIterationDuration = tracerSpan.Duration;

                                _logger.LogError(exception, "Job execution failed after {@Duration}ms.", lastIterationDuration.Milliseconds);
                            }
                        }
                    }

                    InternalEventsScope.Clear();
                    _tracer.Clear();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Unexpected exception while executing \"{jobEntry.JobType.FullName}\" job.");
                }

                var sleepTime = sleepTimeFromAttribute - lastIterationDuration;

                Thread.Sleep(sleepTime < TimeSpan.Zero ? sleepTimeFromAttribute : sleepTime);
            }
        }
    }
}
