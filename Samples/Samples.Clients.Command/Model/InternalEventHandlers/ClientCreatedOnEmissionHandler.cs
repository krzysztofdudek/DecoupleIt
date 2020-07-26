using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.InternalEvents;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.InternalEventHandlers
{
    internal sealed class ClientCreatedOnEmissionHandler : OnEmissionEventHandlerBase<ClientCreated>
    {
        public ClientCreatedOnEmissionHandler([NotNull] ClientsDbContext context)
        {
            _context = context;
        }

        public override async Task HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
        {
            var clientsBasket = new ClientsBasket(@event.ClientId);

            await _context.AddAsync(clientsBasket, cancellationToken);
        }

        [NotNull]
        private readonly ClientsDbContext _context;
    }
}
