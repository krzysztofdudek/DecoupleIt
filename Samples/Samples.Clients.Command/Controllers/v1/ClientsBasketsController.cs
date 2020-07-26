using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Contracts.Services;
using Samples.Clients.Command.Contracts.Services.Dtos;
using Samples.Clients.Command.Model;

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
        public ClientsBasketsController([NotNull] ClientsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientsBasketDto>> GetAll()
        {
            return (await _context.ClientsBaskets.AsNoTracking()
                                  .ToListAsync()).Select(x => new ClientsBasketDto
            {
                Id       = x.Id,
                ClientId = x.ClientId
            });
        }

        [NotNull]
        private readonly ClientsDbContext _context;
    }
}
