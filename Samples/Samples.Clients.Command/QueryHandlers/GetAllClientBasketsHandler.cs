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
    internal sealed class GetAllClientBasketsHandler : QueryHandlerBase<GetAllClientBaskets, GetAllClientBasketsResult>
    {
        public GetAllClientBasketsHandler([NotNull] IClientBasketsRepository clientBasketsRepository)
        {
            _clientBasketsRepository = clientBasketsRepository;
        }

        [ItemNotNull]
        protected override async ValueTask<GetAllClientBasketsResult> HandleAsync(GetAllClientBaskets query, CancellationToken cancellationToken = default)
        {
            var clients = await _clientBasketsRepository.GetAllAsync(cancellationToken);

            return new GetAllClientBasketsResult(clients.Select(x => new GetAllClientBasketsResult.ClientBasket(x!.Id, x.ClientId))
                                                        .ToList());
        }

        [NotNull]
        private readonly IClientBasketsRepository _clientBasketsRepository;
    }
}
