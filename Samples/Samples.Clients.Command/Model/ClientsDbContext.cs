using GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model
{
    [Transient]
    [ProvidesContext]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public sealed class ClientsDbContext : UnitOfWorkDbContext
    {
        [NotNull]
        [ItemNotNull]
        public DbSet<Client> Clients { get; [UsedImplicitly] set; }

        [NotNull]
        [ItemNotNull]
        public DbSet<ClientsBasket> ClientsBaskets { get; [UsedImplicitly] set; }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("sample");

            base.OnConfiguring(optionsBuilder);
        }
    }
}
