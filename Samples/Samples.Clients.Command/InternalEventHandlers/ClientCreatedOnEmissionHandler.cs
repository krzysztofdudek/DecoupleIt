using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Model;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.InternalEventHandlers
{
    internal sealed class ClientCreatedOnEmissionHandler : OnEmissionInternalEventHandlerBase<ClientCreated>
    {
        public ClientCreatedOnEmissionHandler([NotNull] IUnitOfWorkAccessor accessor)
        {
            _accessor = accessor;
        }

        protected override async ValueTask HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            var clientsBasket = new ClientsBasket(@event.ClientId);

            await context.AddAsync(clientsBasket, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _accessor;
    }
}
