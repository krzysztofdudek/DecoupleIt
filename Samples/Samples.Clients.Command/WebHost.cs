using GS.DecoupleIt.AspNetCore.Service;
using GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore;
using GS.DecoupleIt.Scheduling.Quartz.AspNetCore;
using GS.DecoupleIt.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Samples.Clients.Command.Model;

#pragma warning disable 1591

namespace Samples.Clients.Command
{
    public sealed class WebHost : DefaultWebHost
    {
        public static int Main(string[] args)
        {
            return DefaultWebHost.Main<WebHost>(args);
        }

        public override void ConfigureApplication(WebHostBuilderContext context, IApplicationBuilder builder)
        {
            base.ConfigureApplication(context, builder);

            builder.UseContextualUnitOfWork<ClientsDbContext>();

            builder.UseQuartzSchedulingForAspNetCore();
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            serviceCollection.AddContextualUnitOfWorkForAspNetCore<ClientsDbContext>(context.Configuration.AsNotNull());
        }
    }
}
