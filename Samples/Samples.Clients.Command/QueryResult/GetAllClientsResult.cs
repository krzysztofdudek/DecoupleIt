using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Samples.Clients.Command.QueryResult
{
    public sealed class GetAllClientsResult
    {
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<Client> Clients { get; }

        public GetAllClientsResult([NotNull] [ItemNotNull] IReadOnlyCollection<Client> clients)
        {
            Clients = clients;
        }

        public sealed class Client
        {
            public Guid Id { get; }

            public string Name { get; }

            public Client(Guid id, string name)
            {
                Id   = id;
                Name = name;
            }
        }
    }
}
