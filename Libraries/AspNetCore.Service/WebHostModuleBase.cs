using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

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
        public virtual void ConfigureConfiguration(IConfigurationBuilder configurationBuilder) { }

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
        public virtual void ConfigureMvcBuilder(WebHostBuilderContext context, IMvcBuilder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureNewtonsoftJson(WebHostBuilderContext context, JsonSerializerSettings options) { }

        /// <inheritdoc />
        public virtual void ConfigureOperations(WebHostBuilderContext context, DecoupleIt.Operations.Builder builder) { }

        /// <inheritdoc />
        public virtual void ConfigureRoute(WebHostBuilderContext context, RouteOptions options) { }

        /// <inheritdoc />
        public virtual void ConfigureScheduling(WebHostBuilderContext context, DecoupleIt.Scheduling.Options options) { }

        /// <inheritdoc />
        public virtual void ConfigureServices(WebHostBuilderContext context, IServiceCollection serviceCollection) { }

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
    }
}
