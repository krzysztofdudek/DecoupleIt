using System;
using GS.DecoupleIt.AspNetCore.Service;
using GS.DecoupleIt.AspNetCore.Service.UnitOfWork;
using GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Scheduling;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Samples.Clients.Command.Model;
using Builder = GS.DecoupleIt.Contextual.UnitOfWork.Builder;

#pragma warning disable 1591

namespace Samples.Clients.Command
{
    public sealed class WebHost : DefaultWebHost
    {
        public static int Main(string[] args)
        {
            return Main<WebHost>(args);
        }

        public override void ConfigureApplication(WebHostBuilderContext context, IApplicationBuilder builder)
        {
            base.ConfigureApplication(context, builder);

            builder.UseContextualUnitOfWork<ClientsDbContext>();
        }

        public override void ConfigureMigrations(WebHostBuilderContext context, GS.DecoupleIt.Migrations.Builder builder)
        {
            base.ConfigureMigrations(context, builder);

            builder.ConfigureDbContext(dbContextOptionsBuilder =>
            {
                dbContextOptionsBuilder!.UseInMemoryDatabase("sample");

                dbContextOptionsBuilder.ConfigureWarnings(warningsConfigurationBuilder =>
                {
                    warningsConfigurationBuilder!.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
            });
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            var assembly = typeof(WebHost).Assembly;

            serviceCollection.ScanAssemblyForImplementations(assembly);
            serviceCollection.ScanAssemblyForOptions(assembly, context.Configuration.AsNotNull());
            serviceCollection.ScanAssemblyForJobs(assembly);
            serviceCollection.ScanAssemblyForHttpClients(assembly);
        }

        public override void ConfigureTracing(WebHostBuilderContext context, GS.DecoupleIt.Tracing.Builder builder)
        {
            base.ConfigureTracing(context, builder);

            builder.WithConfiguration(configuration =>
            {
                configuration!.NewTracingIdGenerator = () => new TracingId(Guid.NewGuid()
                                                                               .ToString("N")[..16]!);
            });
        }

        public override void ConfigureUnitOfWork(WebHostBuilderContext context, Builder builder)
        {
            base.ConfigureUnitOfWork(context, builder);

            builder.AddSupportForEntityFrameworkCore();

            builder.WithContextMiddlewareFor<ClientsDbContext>();
        }

        protected override bool UseMigrations => false;
    }
}
