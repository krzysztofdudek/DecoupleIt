using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Samples.Clients.Command.Contracts.Events;
using Samples.Clients.Command.Contracts.Exceptions;

#pragma warning disable 1591

namespace Samples.Clients.Command.Model.Entities
{
    public sealed class Client
    {
        public Guid Id { get; [UsedImplicitly] private set; }

        [JetBrains.Annotations.NotNull]
        public string Name { get; [UsedImplicitly] private set; }

        public Client([JetBrains.Annotations.NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidClientName();

            Id   = Guid.NewGuid();
            Name = name;

            new ClientCreated(Id, Name).Emit();
        }

        [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
        private Client() { }
    }
}
