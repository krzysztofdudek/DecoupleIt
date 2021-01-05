using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Samples.Clients.Command.QueryResult
{
    public sealed class GetAllClientBasketsResult
    {
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<ClientBasket> ClientBaskets { get; }

        public GetAllClientBasketsResult([NotNull] [ItemNotNull] IReadOnlyCollection<ClientBasket> clientBaskets)
        {
            ClientBaskets = clientBaskets;
        }

        public sealed class ClientBasket
        {
            public Guid ClientId { get; }

            public Guid Id { get; }

            public ClientBasket(Guid id, Guid clientId)
            {
                Id       = id;
                ClientId = clientId;
            }
        }
    }
}
