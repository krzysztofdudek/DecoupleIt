using System;
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
    public sealed class ClientsDbContext : UnitOfWorkDbContext, IClientsUnitOfWork
    {
        [NotNull]
        [ItemNotNull]
        public DbSet<Client> Clients { get; [UsedImplicitly] set; }

        [NotNull]
        [ItemNotNull]
        public DbSet<ClientsBasket> ClientsBaskets { get; [UsedImplicitly] set; }

        public ClientsDbContext([NotNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("sample");

            optionsBuilder.UseApplicationServiceProvider(_serviceProvider);

            base.OnConfiguring(optionsBuilder);
        }

        [NotNull]
        private readonly IServiceProvider _serviceProvider;
    }
}
