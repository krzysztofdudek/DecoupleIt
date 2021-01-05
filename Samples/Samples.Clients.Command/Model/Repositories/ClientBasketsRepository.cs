using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.Repositories
{
    [Singleton]
    internal sealed class ClientBasketsRepository : IClientBasketsRepository
    {
        public ClientBasketsRepository([NotNull] IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            _unitOfWorkAccessor = unitOfWorkAccessor;
        }

        public async Task<IReadOnlyCollection<ClientsBasket>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var context = _unitOfWorkAccessor.Get<ClientsDbContext>();

            return await context.ClientsBaskets.AsNoTracking()
                                .AsNotNull()
                                .ToListAsync(cancellationToken)
                                .AsNotNull()
                                .WithNotNullItems();
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _unitOfWorkAccessor;
    }
}
