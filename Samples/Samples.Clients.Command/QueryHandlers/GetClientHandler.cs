using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Samples.Clients.Command.Model.Repositories;
using Samples.Clients.Command.Queries;
using Samples.Clients.Command.QueryResult;

namespace Samples.Clients.Command.QueryHandlers
{
    internal sealed class GetClientHandler : QueryHandlerBase<GetClient, GetClientResult>
    {
        public GetClientHandler([NotNull] IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        protected override async ValueTask<GetClientResult> HandleAsync(GetClient query, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(query.Id, cancellationToken);

            return client.Map(x => new GetClientResult(x.Id, x.Name))
                         .ReduceToDefault();
        }

        [NotNull]
        private readonly IClientRepository _clientRepository;
    }
}
