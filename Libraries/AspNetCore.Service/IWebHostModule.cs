#if NETCOREAPP3_1 || NET5_0
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
#elif NETCOREAPP2_2
using Newtonsoft.Json;
#endif
using GS.DecoupleIt.Contextual.UnitOfWork;
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

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Web host module.
    /// </summary>
    [PublicAPI]
    public interface IWebHostModule
    {
        /// <summary>
        ///     Configures <see cref="IApplicationBuilder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Application builder.</param>
        void ConfigureApplication([NotNull] WebHostBuilderContext context, [NotNull] IApplicationBuilder builder);

#if NETCOREAPP3_1 || NET5_0
        /// <summary>
        ///     Configures <see cref="IEndpointRouteBuilder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Endpoint route builder.</param>
        void ConfigureEndpoints([NotNull] WebHostBuilderContext context, [NotNull] IEndpointRouteBuilder builder);

        /// <summary>
        ///     Configures <see cref="JsonOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Json options.</param>
        void ConfigureJson([NotNull] WebHostBuilderContext context, [NotNull] JsonSerializerOptions options);
#elif NETCOREAPP2_2
        /// <summary>
        ///     Configures <see cref="IRouteBuilder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Endpoint route builder.</param>
        void ConfigureEndpoints([NotNull] WebHostBuilderContext context, [NotNull] IRouteBuilder builder);

        /// <summary>
        ///     Configures <see cref="JsonSerializerSettings" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Json options.</param>
        void ConfigureJson([NotNull] WebHostBuilderContext context, [NotNull] JsonSerializerSettings options);
#endif

        /// <summary>
        ///     Configures <see cref="IMvcBuilder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Mvc builder.</param>
        void ConfigureMvcBuilder([NotNull] WebHostBuilderContext context, [NotNull] IMvcBuilder builder);

        /// <summary>
        ///     Configures <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="serviceCollection">Service collection.</param>
        void ConfigureServices([NotNull] WebHostBuilderContext context, [NotNull] IServiceCollection serviceCollection);

        /// <summary>
        ///     Configures <see cref="SwaggerOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Swagger options.</param>
        void ConfigureSwagger([NotNull] WebHostBuilderContext context, [NotNull] SwaggerOptions options);

        /// <summary>
        ///     Configures <see cref="SwaggerGenOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Swagger gen options.</param>
        void ConfigureSwaggerGen([NotNull] WebHostBuilderContext context, [NotNull] SwaggerGenOptions options);

        /// <summary>
        ///     Configures <see cref="SwaggerUIOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Configures swagger UI options.</param>
        void ConfigureSwaggerUI([NotNull] WebHostBuilderContext context, [NotNull] SwaggerUIOptions options);

        /// <summary>
        ///     Configures <see cref="IWebHostBuilder" />.
        /// </summary>
        /// <param name="webHostBuilder">Web host builder.</param>
        void ConfigureWebHostBuilder([NotNull] IWebHostBuilder webHostBuilder);

        /// <summary>
        ///     Configures <see cref="CorsOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Cors options.</param>
        void ConfigureCors([NotNull] WebHostBuilderContext context, [NotNull] CorsOptions options);

        /// <summary>
        ///     Configures <see cref="CorsPolicyBuilder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Cors policy builder.</param>
        void ConfigureCorsPolicyBuilder([NotNull] WebHostBuilderContext context, [NotNull] CorsPolicyBuilder builder);

        /// <summary>
        ///     Configures <see cref="LoggerConfiguration" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="configuration">Logger configuration.</param>
        void ConfigureLogging([NotNull] WebHostBuilderContext context, [NotNull] LoggerConfiguration configuration);

        /// <summary>
        ///     Configures unit of work <see cref="Builder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Unit of work builder.</param>
        void ConfigureUnitOfWork([NotNull] WebHostBuilderContext context, [NotNull] Builder builder);
    }
}
