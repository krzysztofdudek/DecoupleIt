using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Contracts.Exceptions;

#pragma warning disable 1591

namespace Samples.Clients.Command.Model.Entities
{
    public sealed class Client
    {
        [ItemNotNull]
        public static async ValueTask<Client> CreateAsync([NotNull] string name)
        {
            var client = new Client(name);

            await new ClientCreated(client.Id, client.Name).EmitAsync();

            return client;
        }

        public Guid Id { get; [UsedImplicitly] private set; }

        [NotNull]
        public string Name { get; [UsedImplicitly] private set; }

        private Client([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidClientName();

            Id   = Guid.NewGuid();
            Name = name;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        private Client() { }
    }
}
