using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.InternalEvents;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.InternalEventHandlers
{
    internal sealed class ClientCreatedOnEmissionHandler : OnEmissionEventHandlerBase<ClientCreated>
    {
        public ClientCreatedOnEmissionHandler([NotNull] IUnitOfWorkAccessor accessor)
        {
            _accessor = accessor;
        }

        public override async ValueTask HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            var clientsBasket = new ClientsBasket(@event.ClientId);

            await context.AddAsync(clientsBasket, cancellationToken);
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _accessor;
    }
}
