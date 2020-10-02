using GS.DecoupleIt.AspNetCore.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Samples.Clients.Command.Contracts;

#pragma warning disable 1591

namespace Samples.Documents.Command
{
    public sealed class WebHost : DefaultWebHost
    {
        public static int Main(string[] args)
        {
            return DefaultWebHost.Main<WebHost>(args);
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            serviceCollection.AddClientsCommandServices();
        }
    }
}
