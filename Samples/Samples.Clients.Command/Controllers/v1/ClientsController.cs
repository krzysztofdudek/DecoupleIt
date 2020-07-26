using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Contracts.Services;
using Samples.Clients.Command.Contracts.Services.Dtos;
using Samples.Clients.Command.Model;
using Samples.Clients.Command.Model.Entities;

#pragma warning disable 1591

namespace Samples.Clients.Command.Controllers.v1
{
    /// <inheritdoc cref="IClients" />
    [Route("api/v1/clients")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotateNotNullTypeMember")]
    [ApiExplorerSettings(GroupName = "v1")]
    public sealed class ClientsController : ControllerBase, IClients
    {
        public ClientsController([JetBrains.Annotations.NotNull] ClientsDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        [HttpPost]
        public async Task<CreatedClientDto> CreateClient([BindRequired] [FromBody] CreateClientDto dto)
        {
            var client = new Client(dto.Name);

            await _context.Clients.AddAsync(client);

            await _context.SaveChangesAsync();

            return new CreatedClientDto
            {
                Id   = client.Id,
                Name = client.Name
            };
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public async Task<ClientDto> Get(Guid id)
        {
            var client = await _context.Clients.AsNoTracking()
                                       .SingleAsync(x => x.Id == id);

            return new ClientDto
            {
                Id   = client.Id,
                Name = client.Name
            };
        }

        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientDto>> GetAll()
        {
            return (await _context.Clients.AsNoTracking()
                                  .ToListAsync()).Select(x => new ClientDto
            {
                Id   = x.Id,
                Name = x.Name
            });
        }

        [JetBrains.Annotations.NotNull]
        private readonly ClientsDbContext _context;
    }
}
