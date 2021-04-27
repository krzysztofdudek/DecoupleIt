using GS.DecoupleIt.AspNetCore.Service;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Scheduling;
using GS.DecoupleIt.Shared;
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
            return Main<WebHost>(args);
        }

        public override void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            base.ConfigureServices(context, serviceCollection);

            var assembly = typeof(WebHost).Assembly;

            serviceCollection.ScanAssemblyForImplementations(assembly);
            serviceCollection.ScanAssemblyForOptions(assembly, context.Configuration.AsNotNull());
            serviceCollection.ScanAssemblyForJobs(assembly);
            serviceCollection.ScanAssemblyForHttpClients(assembly);

            serviceCollection.AddClientsCommandServices();
        }
    }
}
