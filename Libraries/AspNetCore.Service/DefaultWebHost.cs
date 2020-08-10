using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.InternalEvents.AspNetCore;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using GS.DecoupleIt.Tracing.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
#if NETCOREAPP2_2
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Rewrite;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#elif NETCOREAPP3_1
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

#endif

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Default host configuration with all required base concepts implemented.
    /// </summary>
    [PublicAPI]
    public abstract class DefaultWebHost : WebHostModuleBase
    {
        /// <summary>
        ///     Identifier.
        /// </summary>
        public Guid Identifier { get; }

        /// <summary>
        ///     If this flag is set, https is enforced.
        /// </summary>
        public bool UseHttpsRedirection { get; set; }

        /// <summary>
        ///     Version.
        /// </summary>
        public string Version { get; }

        /// <summary>
        ///     If this flag is set, then web host will test all registered services if are possible to instantiate.
        /// </summary>
        protected virtual bool ValidateServices { get; } = true;

#if NETCOREAPP3_1
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureJson(WebHostBuilderContext context, JsonOptions options)
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
            options.JsonSerializerOptions.IgnoreNullValues = true;
        }
#elif NETCOREAPP2_2
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureJson(WebHostBuilderContext context, JsonSerializerSettings options)
        {
            options.Converters.Add(new StringEnumConverter());
            options.NullValueHandling = NullValueHandling.Ignore;
        }
#endif

        /// <inheritdoc />
        public override void ConfigureSwaggerGen(WebHostBuilderContext context, SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1",
                               new OpenApiInfo
                               {
                                   Title = GetType()
                                           .Assembly.GetName()
                                           .Name,
                                   Version = "v1"
                               });
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming")]
        public override void ConfigureSwaggerUI(WebHostBuilderContext context, SwaggerUIOptions options)
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        }

        /// <inheritdoc cref="WebHostExtensions.Run" />
        /// <param name="args">Arguments.</param>
        public void Run([CanBeNull] [ItemCanBeNull] params string[] args)
        {
            var webHost = CreateWebHost(args);

            webHost.Run();
        }

        /// <inheritdoc cref="IWebHost.Start" />
        /// <param name="args">Arguments.</param>
        /// <returns>Web host.</returns>
        [NotNull]
        public IWebHost Start([CanBeNull] [ItemCanBeNull] params string[] args)
        {
            var webHost = CreateWebHost(args);

            webHost.Start();

            return webHost;
        }

        /// <summary>
        ///     Setups an instance of <see cref="DefaultWebHost" />.
        /// </summary>
        protected DefaultWebHost()
        {
            Identifier = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTIDENTIFIER")
                                    ?.ToLower() == "default"
                ? Guid.Empty
                : Guid.NewGuid();

            Version = GetType()
                      .Assembly.GetName()
                      .Version?.ToString() ?? "undefined";
        }

        /// <summary>
        ///     Gets modules.
        /// </summary>
        /// <returns>Modules.</returns>
        [NotNull]
        [ItemNotNull]
        protected virtual IReadOnlyCollection<IWebHostModule> GetModules()
        {
            return new IWebHostModule[0];
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        [NotNull]
        private IWebHost CreateWebHost(string[] args)
        {
            args = args?.Where(x => x != null)
                       .ToArray() ?? new string[0];

            var modules = GetModules();

            var webHostBuilder = new WebHostBuilder();

            webHostBuilder.UseKestrel()
                          .UseContentRoot(Directory.GetCurrentDirectory())
                          .UseDefaultServiceProvider((context, options) =>
                          {
                              var isDevelopment = context.AsNotNull()
                                                         .HostingEnvironment.IsDevelopment();

                              options.AsNotNull()
                                     .ValidateScopes = isDevelopment;

#if NETCOREAPP3_1
                              options.AsNotNull()
                                     .ValidateOnBuild = isDevelopment;
#endif
                          })
                          .ConfigureAppConfiguration((context, builder) =>
                          {
                              builder.AddJsonFile("appsettings.json", true, true);
                              builder.AddJsonFile("appsettings.user.json", true, true);
                              builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
                              builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.user.json", true, true);

                              builder.AddEnvironmentVariables("DOTNET_");
                              builder.AddCommandLine(args);
                          })
                          .ConfigureLogging((context, builder) =>
                          {
                              builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                              builder.AddConsole();
                              builder.AddDebug();
                          })
                          .ConfigureServices((context, collection) =>
                          {
                              context    = context.AsNotNull();
                              collection = collection.AsNotNull();

                              collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());
                              collection.AddInternalEventsForAspNetCore();

                              collection.ConfigureHttpClients(context.Configuration.AsNotNull(),
                                                              options =>
                                                              {
                                                                  options.HostIdentifier = Identifier;

                                                                  options.HostName = GetType()
                                                                                     .Assembly.GetName()
                                                                                     .Name;

                                                                  options.HostVersion = Version;
                                                              });

#if NETCOREAPP3_1
                              var mvcBuilder = collection.AddControllersWithViews()
                                                         .AddJsonOptions(options =>
                                                         {
                                                             options = options.AsNotNull();

                                                             ConfigureJson(context, options);

                                                             foreach (var module in modules)
                                                                 module.ConfigureJson(context, options);
                                                         })
                                                         .AsNotNull();
#elif NETCOREAPP2_2
                              collection.AddRouting();

                              var mvcBuilder = collection.AddMvc()
                                                         .AddJsonOptions(options =>
                                                         {
                                                             options = options.AsNotNull();

                                                             ConfigureJson(context, options.SerializerSettings.AsNotNull());

                                                             foreach (var module in modules)
                                                                 module.ConfigureJson(context, options.SerializerSettings.AsNotNull());
                                                         })
                                                         .AsNotNull();
#endif

                              ConfigureMvcBuilder(context, mvcBuilder);

                              foreach (var module in modules)
                                  module.ConfigureMvcBuilder(context, mvcBuilder);

                              collection.AddSwaggerGen(options =>
                              {
                                  options = options.AsNotNull();

                                  options.OperationFilter<OperationNameFilter>();

                                  options.CustomSchemaIds(x =>
                                  {
                                      if (x.FullName.Contains('+'))
                                          return x.FullName.Substring(x.FullName.LastIndexOf('.') + 1)
                                                  .Replace("+", "");

                                      return x.Name;
                                  });

                                  options.DocumentFilter<DocumentFilter>(Identifier, Version);

                                  foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(GetType()
                                                                                                      .Assembly.Location))
                                                                .AsNotNull()
                                                                .Where(x => x.ToLower()
                                                                             .TrimEnd()
                                                                             .EndsWith(".xml")))
                                      options.IncludeXmlComments(file);

                                  ConfigureSwaggerGen(context, options);

                                  foreach (var module in modules)
                                      module.ConfigureSwaggerGen(context, options);
                              });

                              collection.AddCors(options =>
                              {
                                  options = options.AsNotNull();

                                  ConfigureCors(context, options);
                              });

                              collection.ScanAssemblyForImplementations(typeof(DefaultWebHost).Assembly);

                              ConfigureServices(context, collection);

                              foreach (var module in modules)
                                  module.ConfigureServices(context, collection);

                              if (ValidateServices)
                                  ValidateServicesIfArePossibleToInstantiate(collection);
                          })
#if NETCOREAPP3_1
                          .Configure((context, applicationBuilder) =>
                          {
                              context = context.AsNotNull();
                              applicationBuilder = applicationBuilder.AsNotNull();
#elif NETCOREAPP2_2
                          .Configure(applicationBuilder =>
                          {
                              var context = new WebHostBuilderContext
                              {
                                  Configuration      = applicationBuilder.ApplicationServices.GetRequiredService<IConfiguration>(),
                                  HostingEnvironment = applicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>()
                              };

                              applicationBuilder = applicationBuilder.AsNotNull();
#endif

                              if (UseHttpsRedirection)
#if NETCOREAPP3_1
                                  applicationBuilder.UseHttpsRedirection();
#elif NETCOREAPP2_2
                                  applicationBuilder.UseRewriter(new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 443));
#endif

                              applicationBuilder.Use(async (context2, next) =>
                              {
                                  var logger = context2.RequestServices.GetRequiredService<ILogger<DefaultWebHost>>();

                                  using (logger.BeginScope(new
                                  {
                                      HostIdentifier = Identifier,
                                      HostName = GetType()
                                                 .Assembly.GetName()
                                                 .Name,
                                      HostVersion = Version
                                  }))
                                  {
                                      await next();
                                  }
                              });

                              applicationBuilder.UseSwagger(options =>
                              {
                                  options = options.AsNotNull();

                                  ConfigureSwagger(context, options);

                                  foreach (var module in modules)
                                      module.ConfigureSwagger(context, options);
                              });

                              applicationBuilder.UseSwaggerUI(options =>
                              {
                                  options = options.AsNotNull();

                                  options.RoutePrefix = "swagger";

                                  ConfigureSwaggerUI(context, options);

                                  foreach (var module in modules)
                                      module.ConfigureSwaggerUI(context, options);
                              });

#if NETCOREAPP3_1
                              applicationBuilder.UseRouting();
#elif NETCOREAPP2_2
                              applicationBuilder.UseEndpointRouting();
#endif

                              applicationBuilder.UseCors(builder =>
                              {
                                  builder = builder.AsNotNull();

                                  ConfigureCorsPolicyBuilder(context, builder);
                              });

                              applicationBuilder.UseTracing();

                              applicationBuilder.UseMiddleware<LoggingMiddleware>();

                              applicationBuilder.UseInternalEvents();

#if NETCOREAPP3_1
                              applicationBuilder.UseEndpoints(builder =>
                              {
                                  builder = builder.AsNotNull();

                                  builder.MapControllers();

                                  ConfigureEndpoints(context, builder);

                                  foreach (var module in modules)
                                      module.ConfigureEndpoints(context, builder);
                              });
#elif NETCOREAPP2_2
                              applicationBuilder.UseMvc(builder =>
                              {
                                  builder = builder.AsNotNull();

                                  ConfigureEndpoints(context, builder);

                                  foreach (var module in modules)
                                      module.ConfigureEndpoints(context, builder);
                              });
#endif

                              ConfigureApplication(context, applicationBuilder);

                              foreach (var module in modules)
                                  module.ConfigureApplication(context, applicationBuilder);
                          });

            ConfigureWebHostBuilder(webHostBuilder);

            foreach (var module in modules)
                module.ConfigureWebHostBuilder(webHostBuilder);

            var webHost = webHostBuilder.Build()
                                        .AsNotNull();

            return webHost;
        }

        private void ValidateServicesIfArePossibleToInstantiate([NotNull] [ItemNotNull] IServiceCollection collection)
        {
            var anyCorrupted = false;

            Tracer.Initialize();

            using (Tracer.OpenRootSpan(GetType(), SpanType.InternalProcess))
            {
                using (var serviceProvider = collection.BuildServiceProvider())
                {
                    foreach (var serviceType in collection.Where(x => x.ServiceType != null)
                                                          .Select(x => x.ServiceType)
                                                          .Where(x => !x.IsGenericType))
                        try
                        {
                            serviceProvider.GetRequiredService(serviceType);
                        }
                        catch
                        {
                            anyCorrupted = true;

                            Console.Error.WriteLine($"ERROR: Service '{serviceType}' is not possible to instantiate.");
                        }
                }
            }

            Tracer.Clear();

            var isTestRun = Environment.GetEnvironmentVariable("ASPNETCORE_TESTRUN")
                                       ?.ToLower() == "true";

            if (anyCorrupted)
            {
                if (isTestRun)
                    Environment.Exit(1);
                else
                    throw new ApplicationException("Application has services registered that are not possible to instantiate.");
            }
            else
            {
                if (isTestRun)
                    Environment.Exit(0);
            }
        }
    }
}
