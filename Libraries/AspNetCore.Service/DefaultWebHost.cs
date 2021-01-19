using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.Contextual.UnitOfWork.AspNetCore;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.HttpAbstraction.AspNetCore;
using GS.DecoupleIt.InternalEvents.AspNetCore;
using GS.DecoupleIt.Scheduling.AspNetCore;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using GS.DecoupleIt.Tracing.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
#if NETCOREAPP2_2
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Rewrite;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
#elif !(NETCOREAPP2_2 || NETSTANDARD2_0)
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

#endif

namespace GS.DecoupleIt.AspNetCore.Service
{
    /// <summary>
    ///     Default host configuration with all required base concepts implemented.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
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

        /// <summary>
        ///     Template of main function that reports errors caused by test run.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <typeparam name="TWebHost">Type of web host.</typeparam>
        /// <returns>Status code.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public static int Main<TWebHost>([CanBeNull] string[] args = default)
            where TWebHost : DefaultWebHost, new()
        {
            var host = new TWebHost();

            try
            {
                host.Run(args);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message?.Replace("\n", " ")}");

                return 1;
            }

            return 0;
        }

#if NETCOREAPP3_1 || NET5_0
        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureJson(WebHostBuilderContext context, JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonStringEnumConverter());
            options.IgnoreReadOnlyProperties = false;
            options.IgnoreNullValues         = true;
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

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureLogging(WebHostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration.MinimumLevel.Debug()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                         .Filter.ByExcluding(logEvent =>
                         {
                             if (logEvent.Level >= LogEventLevel.Warning)
                                 return false;

                             logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue);

                             var sourceContext = sourceContextValue?.ToString()
                                                                   .Trim('"');

                             if (sourceContext is null)
                                 return false;

                             if (sourceContext.StartsWith("Microsoft.AspNetCore"))
                                 return true;

                             if (sourceContext.StartsWith("Quartz"))
                                 return true;

                             return false;
                         })
                         .Filter.ByExcluding(x =>
                         {
                             x.RemovePropertyIfPresent("ActionId");
                             x.RemovePropertyIfPresent("ActionName");
                             x.RemovePropertyIfPresent("SourceContext");

                             x.RemovePropertyIfPresent("CorrelationId");
                             x.RemovePropertyIfPresent("RequestPath");
                             x.RemovePropertyIfPresent("ConnectionId");
                             x.RemovePropertyIfPresent("RequestId");
                             x.RemovePropertyIfPresent("EventId.Id");

                             return false;
                         })
                         .WriteTo.Console(
                             outputTemplate:
                             "[{Timestamp:HH:mm:ss:fff} {Level:u3} | {@SpanType} {@SpanName} | {@TraceId}:{@SpanId}:{@ParentSpanId}]\n{Message:lj}{NewLine}{Exception}");
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
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

#if NETCOREAPP3_1 || NET5_0
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
                          .ConfigureServices((context, collection) =>
                          {
                              context    = context.AsNotNull();
                              collection = collection.AsNotNull();

                              // Configure unit of work.
                              var unitOfWorkBuilder = collection.AddContextualUnitOfWork(context.Configuration.AsNotNull());

                              ConfigureUnitOfWork(context, unitOfWorkBuilder);

                              foreach (var module in modules)
                                  module.ConfigureUnitOfWork(context, unitOfWorkBuilder);

                              // Configure tracing.
                              collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());

                              // Configure internal events.
                              collection.AddInternalEventsForAspNetCore();

                              // Configure http clients.
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                              var jsonSerializerOptions = new JsonSerializerOptions();

                              ConfigureJson(context, jsonSerializerOptions);
#else
                              var jsonSerializerSettings = new JsonSerializerSettings();

                              ConfigureJson(context, jsonSerializerSettings);
#endif

                              collection.AddHttpClients(context.Configuration.AsNotNull())
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                                        .UseSystemTextJsonResponseDeserializer(jsonSerializerOptions)
                                        .UseSystemTextJsonRequestBodySerializer(jsonSerializerOptions)
#else
                                        .UseNewtonsoftJsonResponseDeserializer(jsonSerializerSettings)
                                        .UseNewtonsoftJsonRequestBodySerializer(jsonSerializerSettings)
#endif
                                  ;

                              collection.PostConfigure<HttpAbstractionOptions>(options =>
                              {
                                  options.HostIdentifier = Identifier;

                                  options.HostName = GetType()
                                                     .Assembly.GetName()
                                                     .Name;

                                  options.HostVersion = Version;
                              });

                              // Configure MVC.
#if NETCOREAPP3_1 || NET5_0
                              var mvcBuilder = collection.AddControllersWithViews()
                                                         .AddJsonOptions(options =>
                                                         {
                                                             options = options.AsNotNull();

                                                             ConfigureJson(context, options.JsonSerializerOptions);

                                                             foreach (var module in modules)
                                                                 module.ConfigureJson(context, options.JsonSerializerOptions);
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

                              // Configure swagger.
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
                                                                                                      .Assembly.Location)
                                                                                    .AsNotNull())
                                                                .AsNotNull()
                                                                .Where(x => x.ToLower()
                                                                             .TrimEnd()
                                                                             .EndsWith(".xml")))
                                      options.IncludeXmlComments(file);

                                  ConfigureSwaggerGen(context, options);

                                  foreach (var module in modules)
                                      module.ConfigureSwaggerGen(context, options);
                              });

                              // Configure cors.
                              collection.AddCors(options =>
                              {
                                  options = options.AsNotNull();

                                  ConfigureCors(context, options);
                              });

                              // Configure services.
                              collection.ScanAssemblyForImplementations(typeof(DefaultWebHost).Assembly);

                              ConfigureServices(context, collection);

                              foreach (var module in modules)
                                  module.ConfigureServices(context, collection);

                              if (ValidateServices)
                                  ValidateServicesIfArePossibleToInstantiate(collection);
                          })
#if NETCOREAPP3_1 || NET5_0
                          .Configure((context, applicationBuilder) =>
                          {
                              context            = context.AsNotNull();
                              applicationBuilder = applicationBuilder.AsNotNull();
#elif NETCOREAPP2_2
                          .Configure(applicationBuilder =>
                          {
                              var context = new WebHostBuilderContext
                              {
                                  Configuration = applicationBuilder.ApplicationServices.GetRequiredService<IConfiguration>(),
                                  HostingEnvironment = applicationBuilder.ApplicationServices.GetRequiredService<IHostingEnvironment>()
                              };

                              applicationBuilder = applicationBuilder.AsNotNull();
#endif

                              if (UseHttpsRedirection)
#if NETCOREAPP3_1 || NET5_0
                                  applicationBuilder.UseHttpsRedirection();
#elif NETCOREAPP2_2
                                  applicationBuilder.UseRewriter(new RewriteOptions().AddRedirectToHttps(StatusCodes.Status301MovedPermanently, 443));
#endif

                              applicationBuilder.Use(async (context2, next) =>
                              {
                                  var logger = context2.RequestServices.GetRequiredService<ILogger<DefaultWebHost>>();

                                  var httpAbstractionOptions = context2.RequestServices.GetRequiredService<IOptions<HttpAbstractionOptions>>()
                                                                       .Value;

                                  var hostName = GetType()
                                                 .Assembly.GetName()
                                                 .Name;

                                  context2.Response.OnStarting(() =>
                                  {
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostIdentifierHeaderName, Identifier.ToString());
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostNameHeaderName, hostName);
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostVersionHeaderName, Version);

                                      return Task.CompletedTask;
                                  });

                                  using (logger.BeginScope(new SelfDescribingDictionary<string, object>
                                  {
                                      {
                                          "HostIdentifier", Identifier
                                      },
                                      {
                                          "HostName", hostName
                                      },
                                      {
                                          "HostVersion", Version
                                      }
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

#if NETCOREAPP3_1 || NET5_0
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

                              applicationBuilder.MaintainStorageOfContextualUnitOfWork();

                              applicationBuilder.UseInternalEvents();

                              ConfigureApplication(context, applicationBuilder);

                              foreach (var module in modules)
                                  module.ConfigureApplication(context, applicationBuilder);

#if NETCOREAPP3_1 || NET5_0
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

                              applicationBuilder.UseDefaultScheduling();
                          })
                          .UseSerilog((context, configuration) =>
                          {
                              ConfigureLogging(context, configuration);

                              foreach (var module in modules)
                                  module.ConfigureLogging(context, configuration);
                          });

            ConfigureWebHostBuilder(webHostBuilder);

            foreach (var module in modules)
                module.ConfigureWebHostBuilder(webHostBuilder);

            var webHost = webHostBuilder.Build()
                                        .AsNotNull();

            return webHost;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        private void ValidateServicesIfArePossibleToInstantiate([NotNull] [ItemNotNull] IServiceCollection collection)
        {
            var anyCorrupted = false;

            using (var serviceProvider = collection.BuildServiceProvider()
                                                   .AsNotNull())
            {
                var tracer = serviceProvider.GetRequiredService<ITracer>()
                                            .AsNotNull();

                using (tracer.OpenSpan(GetType(), SpanType.InternalProcess))
                {
                    foreach (var serviceType in collection.Where(x => x.ServiceType != null)
                                                          .Select(x => x.ServiceType)
                                                          .Where(x => !x.IsGenericType))
                        try
                        {
                            serviceProvider.GetRequiredService(serviceType.AsNotNull());
                        }
                        catch (Exception exception)
                        {
                            anyCorrupted = true;

                            var message = exception.Message is null ? null : Regex.Replace(exception.Message, @"\s+", " ");

                            var stackTrace = exception.StackTrace is null ? null : Regex.Replace(exception.StackTrace, @"\s+", " ");

                            Console.Error.WriteLine($"ERROR: {message} | StackTrace: {stackTrace}");
                        }
                }
            }

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
