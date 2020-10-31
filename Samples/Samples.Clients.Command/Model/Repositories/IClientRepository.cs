using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Optionals;
using JetBrains.Annotations;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.Repositories
{
    [ProvidesContext]
    public interface IClientRepository
    {
        [NotNull]
        Task AddAsync([NotNull] Client client, CancellationToken cancellationToken = default);

        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<Client>> GetAllAsync(CancellationToken cancellationToken = default);

        [NotNull]
        [ItemNotNull]
        Task<Optional<Client>> GetAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
