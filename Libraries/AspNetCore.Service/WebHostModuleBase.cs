using System.Reflection;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Scheduling;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using GS.DecoupleIt.HttpAbstraction;
using Newtonsoft.Json;
using System.Text.Json;

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Base web host module.
    /// </summary>
    public abstract class WebHostModuleBase : IWebHostModule
    {
        /// <inheritdoc />
        public virtual void ConfigureApplication(WebHostBuilderContext context, IApplicationBuilder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureCors(WebHostBuilderContext context, CorsOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureCorsPolicyBuilder(WebHostBuilderContext context, CorsPolicyBuilder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureEndpoints(WebHostBuilderContext context, IEndpointRouteBuilder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureLogging(WebHostBuilderContext context, LoggerConfiguration configuration) { }

        /// <inheritdoc />
        public virtual void ConfigureMigrations(WebHostBuilderContext context, DecoupleIt.Migrations.Builder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureMvcBuilder(WebHostBuilderContext context, IMvcBuilder builder)
        {
            builder.AddApplicationPart(ThisAssembly);
        }

        /// <inheritdoc />
        public virtual void ConfigureNewtonsoftJson(WebHostBuilderContext context, JsonSerializerSettings options) { }

        /// <inheritdoc />
        public virtual void ConfigureOperations(WebHostBuilderContext context, DecoupleIt.Operations.Builder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureScheduling(WebHostBuilderContext context, DecoupleIt.Scheduling.Options options) { }

        /// <inheritdoc />
        public virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection)
        {
            serviceCollection.ScanAssemblyForImplementations(ThisAssembly);
            serviceCollection.ScanAssemblyForOptions(ThisAssembly, context.Configuration.AsNotNull());
            serviceCollection.ScanAssemblyForJobs(ThisAssembly);
            serviceCollection.ScanAssemblyForHttpClients(ThisAssembly);
        }

        /// <inheritdoc />
        public virtual void ConfigureSwagger(WebHostBuilderContext context, SwaggerOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureSwaggerGen(WebHostBuilderContext context, SwaggerGenOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureSwaggerUI(WebHostBuilderContext context, SwaggerUIOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureSystemTextJson(WebHostBuilderContext context, JsonSerializerOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureUnitOfWork(WebHostBuilderContext context, Contextual.UnitOfWork.Builder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder) { }

        /// <summary>
        ///     Assembly that contains this module.
        /// </summary>
        [NotNull]
        [PublicAPI]
        protected Assembly ThisAssembly =>
            GetType()
                .Assembly.AsNotNull();
    }
}
