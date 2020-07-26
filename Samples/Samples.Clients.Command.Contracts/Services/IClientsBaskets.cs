using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestEase;
using Samples.Clients.Command.Contracts.Services.Dtos;

namespace Samples.Clients.Command.Contracts.Services
{
    /// <summary>
    ///     Service allows to manage client baskets.
    /// </summary>
    [PublicAPI]
    [BasePath("api/v1")]
    public interface IClientsBaskets
    {
        /// <summary>
        ///     Gets all clients' baskets.
        /// </summary>
        /// <returns></returns>
        [NotNull]
        [ItemNotNull]
        [Get("clients-baskets")]
        Task<IEnumerable<ClientsBasketDto>> GetAll();
    }
}
