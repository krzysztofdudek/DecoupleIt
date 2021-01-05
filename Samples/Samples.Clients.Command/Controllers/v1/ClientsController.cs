using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Samples.Clients.Command.Contracts.Services;
using Samples.Clients.Command.Contracts.Services.Dtos;
using Samples.Clients.Command.Queries;

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
        /// <inheritdoc />
        [HttpPost]
        public async Task<CreatedClientDto> CreateClient([BindRequired] [FromBody] CreateClientDto dto)
        {
            var result = await new Commands.CreateClient(dto.Name).DispatchAsync(HttpContext.RequestAborted);

            return new CreatedClientDto
            {
                Id   = result.Id,
                Name = result.Name
            };
        }

        /// <inheritdoc />
        [HttpGet("{id}")]
        public async Task<ClientDto> Get(Guid id)
        {
            var result = await new GetClient(id).DispatchAsync(HttpContext.RequestAborted);

            if (result is null)
            {
                HttpContext.Response.StatusCode = 404;

                return null;
            }

            return new ClientDto
            {
                Id   = result.Id,
                Name = result.Name
            };
        }

        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientDto>> GetAll()
        {
            var result = await new GetAllClients().DispatchAsync(HttpContext.RequestAborted);

            return result.Clients.Select(x => new ClientDto
            {
                Id   = x.Id,
                Name = x.Name
            });
        }
    }
}
