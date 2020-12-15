using GS.DecoupleIt.AspNetCore.Service;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore;
using GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Samples.Clients.Command.Model;

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

        public override void ConfigureUnitOfWork(WebHostBuilderContext context, Builder builder)
        {
            base.ConfigureUnitOfWork(context, builder);

            builder.AddEntityFrameworkCore()
                   .AddContextMiddlewareFor<ClientsDbContext>();
        }
    }
}
