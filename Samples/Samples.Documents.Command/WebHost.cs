using GS.DecoupleIt.AspNetCore.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Samples.Clients.Command.Contracts;

#pragma warning disable 1591

namespace Samples.Documents.Command
{
    public sealed class WebHost : DefaultWebHost
    {
        public static void Main(string[] args)
        {
            var host = new WebHost();

            host.Run(args);
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            serviceCollection.AddClientsCommandServices();
        }
    }
}
