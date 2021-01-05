using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.Repositories
{
    [ProvidesContext]
    public interface IClientBasketsRepository
    {
        [NotNull]
        [ItemNotNull]
        Task<IReadOnlyCollection<ClientsBasket>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
