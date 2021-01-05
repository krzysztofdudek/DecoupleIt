﻿using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Operations;
using JetBrains.Annotations;
using Samples.Clients.Command.CommandResults;
using Samples.Clients.Command.Commands;
using Samples.Clients.Command.Model;
using Samples.Clients.Command.Model.Entities;
using Samples.Clients.Command.Model.Repositories;

namespace Samples.Clients.Command.CommandHandlers
{
    internal sealed class CreateClientHandler : CommandHandlerBase<CreateClient, CreateClientResult>
    {
        public CreateClientHandler([NotNull] IUnitOfWorkAccessor unitOfWorkAccessor, [NotNull] IClientRepository clientRepository)
        {
            _unitOfWorkAccessor = unitOfWorkAccessor;
            _clientRepository   = clientRepository;
        }

        [ItemNotNull]
        protected override async ValueTask<CreateClientResult> HandleAsync(CreateClient command, CancellationToken cancellationToken = default)
        {
            await using var unitOfWork = _unitOfWorkAccessor.Get<ClientsDbContext>();

            var client = await Client.CreateAsync(command.Name);

            await _clientRepository.AddAsync(client, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateClientResult(client.Id, client.Name);
        }

        [NotNull]
        private readonly IClientRepository _clientRepository;

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}
