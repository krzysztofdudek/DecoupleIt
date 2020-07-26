using GS.DecoupleIt.AspNetCore.Service;

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
    }
}
