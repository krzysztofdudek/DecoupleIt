using GS.DecoupleIt.AspNetCore.Service;
using GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore;
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
        public static void Main(string[] args)
        {
            var host = new WebHost();

            host.Run(args);
        }

        public override void ConfigureApplication(WebHostBuilderContext context, IApplicationBuilder builder)
        {
            base.ConfigureApplication(context, builder);

            builder.UseContextualUnitOfWork<ClientsDbContext>();
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            serviceCollection.AddContextualUnitOfWorkForAspNetCore<ClientsDbContext>(context.Configuration.AsNotNull());
        }
    }
}
