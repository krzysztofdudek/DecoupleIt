using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Scheduling;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Samples.Clients.Command.Model;

namespace Samples.Clients.Command.Jobs
{
    [Singleton]
    [SimpleSchedule(Milliseconds = 1)]
    internal sealed class SampleJob : IJob
    {
        public SampleJob([NotNull] ILogger<SampleJob> logger, [NotNull] IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            _logger             = logger;
            _unitOfWorkAccessor = unitOfWorkAccessor;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sample job log.");

            await using var dbContext = _unitOfWorkAccessor.Get<ClientsDbContext>();
        }

        [NotNull]
        private readonly ILogger<SampleJob> _logger;

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}
