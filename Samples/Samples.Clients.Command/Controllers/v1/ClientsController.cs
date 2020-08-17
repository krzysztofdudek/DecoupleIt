using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Samples.Clients.Command.Contracts.Services;
using Samples.Clients.Command.Contracts.Services.Dtos;
using Samples.Clients.Command.Model;
using Samples.Clients.Command.Model.Entities;
using Samples.Clients.Command.Model.Repositories;

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
        public ClientsController([NotNull] IUnitOfWorkAccessor unitOfWorkAccessor, [NotNull] IClientRepository clientRepository)
        {
            _unitOfWorkAccessor = unitOfWorkAccessor;
            _clientRepository   = clientRepository;
        }

        /// <inheritdoc />
        [HttpPost]
        public async Task<CreatedClientDto> CreateClient([BindRequired] [FromBody] CreateClientDto dto)
        {
            await using var unitOfWork = _unitOfWorkAccessor.Get<ClientsDbContext>();

            var client = new Client(dto.Name);

            await _clientRepository.AddAsync(client);

            await unitOfWork.SaveChangesAsync();

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
            await using var _ = _unitOfWorkAccessor.Get<ClientsDbContext>();

            var client = await _clientRepository.GetAsync(id);

            return client.Map(x => new ClientDto
                         {
                             Id   = x.Id,
                             Name = x.Name
                         })
                         .Reduce(new ClientDto());
        }

        /// <inheritdoc />
        [HttpGet]
        public async Task<IEnumerable<ClientDto>> GetAll()
        {
            await using var _ = _unitOfWorkAccessor.Get<ClientsDbContext>();

            var clients = await _clientRepository.GetAllAsync();

            return clients.Select(x => new ClientDto
            {
                Id   = x.Id,
                Name = x.Name
            });
        }

        [NotNull]
        private readonly IClientRepository _clientRepository;

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}
