using System;
using JetBrains.Annotations;

namespace Samples.Clients.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Client.
    /// </summary>
    [PublicAPI]
    public sealed class ClientDto
    {
        /// <summary>
        ///     Identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; set; }
    }
}
