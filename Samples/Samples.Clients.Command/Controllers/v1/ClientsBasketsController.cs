using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Samples.Clients.Command.Contracts.Services;
using Samples.Clients.Command.Contracts.Services.Dtos;
using Samples.Clients.Command.Queries;

#pragma warning disable 1591

namespace Samples.Clients.Command.Controllers.v1
{
    /// <inheritdoc cref="IClientsBaskets" />
    [Route("api/v1/clients-baskets")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotateNotNullTypeMember")]
    [ApiExplorerSettings(GroupName = "v1")]
    public sealed class ClientsBasketsController : ControllerBase, IClientsBaskets
    {
        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientsBasketDto>> GetAll()
        {
            var clientBaskets = await new GetAllClientBaskets().DispatchAsync(HttpContext.RequestAborted);

            return clientBaskets.ClientBaskets.Select(x => new ClientsBasketDto
            {
                Id       = x.Id,
                ClientId = x.ClientId
            });
        }
    }
}
