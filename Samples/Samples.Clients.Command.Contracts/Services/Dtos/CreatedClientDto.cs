using System;
using JetBrains.Annotations;

namespace Samples.Clients.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Dto used to return information about created client.
    /// </summary>
    [PublicAPI]
    public sealed class CreatedClientDto
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
