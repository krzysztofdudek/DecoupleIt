using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Samples.Clients.Command.Model.Entities;

#pragma warning disable 1591

namespace Samples.Clients.Command.Model
{
    [Scoped]
    [RegisterAsSelf]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "NotNullMemberIsNotInitialized")]
    public sealed class ClientsDbContext : DbContext
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
