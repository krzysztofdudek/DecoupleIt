using System;
using GS.DecoupleIt.Persistence.Automatic;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Contracts.Exceptions;

#pragma warning disable 1591

namespace Samples.Clients.Command.Model.Entities
{
    [Persist]
    public sealed class Client
    {
        public Guid Id { get; [UsedImplicitly] private set; }

        [NotNull]
        public string Name { get; [UsedImplicitly] private set; }

        public Client([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidClientName();

            Id   = Guid.NewGuid();
            Name = name;

            new ClientCreated(Id, Name).Emit();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        private Client() { }
    }
}
