using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Scheduling;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Samples.Clients.Command.Jobs
{
    [Singleton]
    [SimpleSchedule(Seconds = 5)]
    internal sealed class SampleJob : IJob
    {
        public SampleJob([NotNull] ILogger<SampleJob> logger)
        {
            _logger = logger;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sample job log.");

            return Task.CompletedTask;
        }

        [NotNull]
        private readonly ILogger<SampleJob> _logger;
    }
}
