using System;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;

namespace Samples.Clients.Command.Contracts.Events
{
    /// <summary>
    ///     Event is emitted when client is created.
    /// </summary>
    [PublicAPI]
    public sealed class ClientCreated : InternalEvent
    {
        /// <summary>
        ///     Client's identifier.
        /// </summary>
        public Guid ClientId { get; }

        /// <summary>
        ///     Name of an client.
        /// </summary>
        [NotNull]
        public string Name { get; }

        internal ClientCreated(Guid clientId, [NotNull] string name)
        {
            ClientId = clientId;
            Name     = name;
        }
    }
}
