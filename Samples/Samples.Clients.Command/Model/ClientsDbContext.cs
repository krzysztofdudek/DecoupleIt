using System.Diagnostics.CodeAnalysis;
using GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Model.Entities;

namespace Samples.Clients.Command.Model
{
    [Transient]
    [ProvidesContext]
    [SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public sealed class ClientsDbContext : UnitOfWorkDbContext
    {
        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public DbSet<Client> Clients { get; [UsedImplicitly] set; }

        [JetBrains.Annotations.NotNull]
        [ItemNotNull]
        public DbSet<ClientsBasket> ClientsBaskets { get; [UsedImplicitly] set; }

        protected override void OnConfiguring([JetBrains.Annotations.NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("sample");

            base.OnConfiguring(optionsBuilder);
        }
    }
}
