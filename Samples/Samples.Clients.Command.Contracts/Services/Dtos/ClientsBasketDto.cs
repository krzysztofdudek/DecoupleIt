using System;
using JetBrains.Annotations;

namespace Samples.Clients.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Client's basket.
    /// </summary>
    [PublicAPI]
    public sealed class ClientsBasketDto
    {
        /// <summary>
        ///     Client's identifier.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        ///     Identifier.
        /// </summary>
        public Guid Id { get; set; }
    }
}
