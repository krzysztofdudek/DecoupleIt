using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.InternalEvents;
using Samples.Clients.Command.Contracts.Events;

namespace Samples.Clients.Command.Model.InternalEventHandlers
{
    internal sealed class ClientCreatedOnSuccessHandler : OnSuccessEventHandlerBase<ClientCreated>
    {
        public override Task HandleAsync(ClientCreated @event, CancellationToken cancellationToken = default)
        {
            Console.Write(@event.ClientId);

            return Task.CompletedTask;
        }
    }
}
