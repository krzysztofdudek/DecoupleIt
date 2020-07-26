using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestEase;
using Samples.Clients.Command.Contracts.Services.Dtos;

namespace Samples.Clients.Command.Contracts.Services
{
    /// <summary>
    ///     Service allows to manage clients.
    /// </summary>
    [PublicAPI]
    [BasePath("api/v1/clients")]
    public interface IClients
    {
        /// <summary>
        ///     Creates client.
        /// </summary>
        /// <param name="dto">Data.</param>
        /// <returns>Created client.</returns>
        [NotNull]
        [ItemNotNull]
        [Post]
        Task<CreatedClientDto> CreateClient([NotNull] [Body] CreateClientDto dto);

        /// <summary>
        ///     Gets client with specific identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <returns>Client.</returns>
        [NotNull]
        [ItemNotNull]
        [Get("{id}")]
        Task<ClientDto> Get([Path] Guid id);

        /// <summary>
        ///     Gets all clients.
        /// </summary>
        /// <returns>Clients.</returns>
        [NotNull]
        [ItemNotNull]
        [Get]
        Task<IEnumerable<ClientDto>> GetAll();
    }
}
