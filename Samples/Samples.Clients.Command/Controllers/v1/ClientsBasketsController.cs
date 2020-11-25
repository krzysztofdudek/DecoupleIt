using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
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
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [SuppressMessage("ReSharper", "AnnotateNotNullTypeMember")]
    [ApiExplorerSettings(GroupName = "v1")]
    public sealed class ClientsBasketsController : ControllerBase, IClientsBaskets
    {
        public ClientsBasketsController([JetBrains.Annotations.NotNull] IUnitOfWorkAccessor accessor)
        {
            _accessor = accessor;
        }

        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientsBasketDto>> GetAll()
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            return (await context.ClientsBaskets.AsNoTracking()
                                 .ToListAsync()).Select(x => new ClientsBasketDto
            {
                Id       = x.Id,
                ClientId = x.ClientId
            });
        }

        [JetBrains.Annotations.NotNull]
        private readonly IUnitOfWorkAccessor _accessor;
    }
}
