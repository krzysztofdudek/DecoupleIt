using System;
using System.Collections.Generic;
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

        public async Task AddAsync(Client client)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            await context.AddAsync(client);
        }

        public async Task<IReadOnlyCollection<Client>> GetAllAsync()
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            return await context.Clients.ToListAsync()
                                .AsNotNull()
                                .WithNotNullItems();
        }

        public async Task<Optional<Client>> GetAsync(Guid id)
        {
            await using var context = _accessor.Get<ClientsDbContext>();

            return await context.Clients.FirstOrDefaultAsync(x => x.Id == id)
                                .AsOptional();
        }

        [NotNull]
        private readonly IUnitOfWorkAccessor _accessor;
    }
}
