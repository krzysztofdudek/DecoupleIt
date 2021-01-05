using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Samples.Clients.Command.Model.Repositories;
using Samples.Clients.Command.Queries;
using Samples.Clients.Command.QueryResult;

namespace Samples.Clients.Command.QueryHandlers
{
    internal sealed class GetAllClientsHandler : QueryHandlerBase<GetAllClients, GetAllClientsResult>
    {
        public GetAllClientsHandler([NotNull] IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [ItemNotNull]
        protected override async ValueTask<GetAllClientsResult> HandleAsync(GetAllClients query, CancellationToken cancellationToken = default)
        {
            var clients = await _clientRepository.GetAllAsync(cancellationToken);

            return new GetAllClientsResult(clients.Select(x => new GetAllClientsResult.Client(x!.Id, x.Name))
                                                  .ToList());
        }

        [NotNull]
        private readonly IClientRepository _clientRepository;
    }
}
