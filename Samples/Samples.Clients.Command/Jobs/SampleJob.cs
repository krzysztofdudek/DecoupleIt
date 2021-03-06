using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Scheduling;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Samples.Clients.Command.Jobs
{
    [CyclicSchedule(Seconds = 5)]
    internal sealed class SampleJob : IJob
    {
        public SampleJob([NotNull] ILogger<SampleJob> logger)
        {
            _logger = logger;
        }

        public ValueTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Job");

            return new ValueTask();
        }

        [NotNull]
        private readonly ILogger<SampleJob> _logger;
    }
}
