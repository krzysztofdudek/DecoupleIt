using System;
using GS.DecoupleIt.Persistence.Automatic;
using JetBrains.Annotations;

#pragma warning disable 1591

namespace Samples.Clients.Command.Model.Entities
{
    [Persist]
    public sealed class ClientsBasket
    {
        public Guid ClientId { get; [UsedImplicitly] private set; }

        public Guid Id { get; [UsedImplicitly] private set; }

        public ClientsBasket(Guid clientId)
        {
            Id       = Guid.NewGuid();
            ClientId = clientId;
        }

        private ClientsBasket() { }
    }
}
