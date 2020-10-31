using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Optionals;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model.Repositories
{
    [Singleton]
    internal sealed class ClientRepository : IClientRepository
    {
        public ClientRepository([NotNull] IUnitOfWorkAccessor accessor)
        {
            _accessor = accessor;
        }

        public async Task AddAsync(Client client, CancellationToken cancellationToken = default)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            await context.AddAsync(client, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Client>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            return await context.Clients.AsNoTracking()
                                .AsNotNull()
                                .ToListAsync(cancellationToken)
                                .AsNotNull()
                                .WithNotNullItems();
        }

        public async Task<Optional<Client>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            return await context.Clients.AsNoTracking()
                                .AsNotNull()
                                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                                .AsOptional();
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _accessor;
    }
}
