using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GS.DecoupleIt.AspNetCore.Service.HttpAbstraction;
using GS.DecoupleIt.AspNetCore.Service.Migrations;
using GS.DecoupleIt.AspNetCore.Service.Operations;
using GS.DecoupleIt.AspNetCore.Service.Scheduling;
using GS.DecoupleIt.AspNetCore.Service.Tracing;
using GS.DecoupleIt.AspNetCore.Service.UnitOfWork;
using GS.DecoupleIt.Contextual.UnitOfWork;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.HttpAbstraction;
using GS.DecoupleIt.Migrations;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Scheduling;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
#if !NETSTANDARD2_0
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

        /// <summary>
        ///     Host information summary.
        /// </summary>
        [CanBeNull]
        public IHostInformation HostInformation { get; private set; }

        /// <summary>
        ///     If this flag is set, https is enforced.
        /// </summary>
        public bool UseHttpsRedirection { get; set; }

        /// <summary>
        ///     If this flag is set, migration engine is enabled.
        /// </summary>
        public bool UseMigrations { get; set; }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureLogging(WebHostBuilderContext context, LoggerConfiguration configuration)
        {
            var builder = configuration.MinimumLevel.Debug()
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
                                       });

            var options = context.Configuration.GetValue<bool?>("GS:DecoupleIt:AspNetCore:Service:Logging:Console:Enabled");

            if (options != false)
                builder.WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss:fff} {Level:u3} | {@SpanType} {@SpanName} | {@TraceId}:{@SpanId}:{@ParentSpanId}]\n{Message:lj}{NewLine}{Exception}");
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureNewtonsoftJson(WebHostBuilderContext context, JsonSerializerSettings options)
        {
            options.Converters.Add(new StringEnumConverter());
            options.NullValueHandling = NullValueHandling.Ignore;
        }

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
        public override void ConfigureSystemTextJson(WebHostBuilderContext context, JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonStringEnumConverter());
            options.IgnoreReadOnlyProperties = false;
            options.IgnoreNullValues         = true;
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
        ///     Type of an json serializer used for whole pipeline.
        /// </summary>
        public enum JsonSerializerType
        {
            NewtonsoftJson,
            SystemTextJson
        }

        /// <summary>
        ///     Type of an json serializer used for whole pipeline.
        /// </summary>
        protected virtual JsonSerializerType JsonSerializer { get; } = JsonSerializerType.SystemTextJson;

        /// <summary>
        ///     If this flag is set, then web host will test all registered services if are possible to instantiate.
        /// </summary>
        protected virtual bool ValidateServices { get; } = true;

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

                              options.AsNotNull()
                                     .ValidateOnBuild = isDevelopment;
                          })
                          .ConfigureAppConfiguration((context, builder) =>
                          {
                              builder.AddJsonFile("appsettings.json", true, true);
                              builder.AddJsonFile("appsettings.user.json", true, true);
                              builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
                              builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.user.json", true, true);

                              builder.AddEnvironmentVariables("DOTNET_");
                              builder.AddCommandLine(args);

                              ConfigureConfiguration(builder);

                              foreach (var module in modules)
                                  module.ConfigureConfiguration(builder);
                          })
                          .ConfigureServices((context, collection) =>
                          {
                              var hostIdentifier = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTIDENTIFIER")
                                                              ?.ToLower() == "default"
                                  ? Guid.Empty
                                  : Guid.NewGuid();

                              var hostName = GetType()
                                             .Assembly.GetName()
                                             .Name;

                              var hostVersion = GetType()
                                                .Assembly.GetName()
                                                .Version?.ToString() ?? "undefined";

                              var hostEnvironment = context.HostingEnvironment.EnvironmentName;

                              HostInformation = new HostInformation(hostIdentifier,
                                                                    hostName,
                                                                    hostVersion,
                                                                    hostEnvironment);

                              collection.Add(ServiceDescriptor.Singleton(typeof(IHostInformation), HostInformation));

                              // Configure automatic dependency injection.
                              collection.ConfigureAutomaticDependencyInjection(context.Configuration,
                                                                               options =>
                                                                               {
                                                                                   options.Environment = context.HostingEnvironment.EnvironmentName;
                                                                               });

                              // Configure scheduling.
                              collection.ConfigureJobs(context.Configuration,
                                                       options =>
                                                       {
                                                           ConfigureScheduling(context, options);

                                                           foreach (var module in modules)
                                                               module.ConfigureScheduling(context, options);
                                                       });

                              // Configure unit of work.
                              var unitOfWorkBuilder = collection.AddContextualUnitOfWork(context.Configuration.AsNotNull());

                              ConfigureUnitOfWork(context, unitOfWorkBuilder);

                              foreach (var module in modules)
                                  module.ConfigureUnitOfWork(context, unitOfWorkBuilder);

                              // Configure tracing.
                              collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());

                              // Configure operations.
                              var operationsBuilder = collection.AddOperationsForAspNetCore(context.Configuration.AsNotNull());

                              ConfigureOperations(context, operationsBuilder);

                              foreach (var module in modules)
                                  module.ConfigureOperations(context, operationsBuilder);

                              // Configure json serializer.
                              var jsonSerializerSettings = new JsonSerializerSettings();
                              var jsonSerializerOptions  = new JsonSerializerOptions();

                              switch (JsonSerializer)
                              {
                                  case JsonSerializerType.NewtonsoftJson:
                                      ConfigureNewtonsoftJson(context, jsonSerializerSettings);

                                      break;
                                  case JsonSerializerType.SystemTextJson:
                                      ConfigureSystemTextJson(context, jsonSerializerOptions);

                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              // Configure http clients.
                              var httpClientBuilder = collection.AddHttpClients(context.Configuration.AsNotNull());

                              switch (JsonSerializer)
                              {
                                  case JsonSerializerType.NewtonsoftJson:
                                      httpClientBuilder.UseNewtonsoftJsonResponseDeserializer(jsonSerializerSettings)
                                                       .UseNewtonsoftJsonRequestBodySerializer(jsonSerializerSettings);

                                      break;
                                  case JsonSerializerType.SystemTextJson:
                                      httpClientBuilder.UseSystemTextJsonResponseDeserializer(jsonSerializerOptions)
                                                       .UseSystemTextJsonRequestBodySerializer(jsonSerializerOptions);

                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              // Configure MVC.
                              var mvcBuilder = collection.AddControllersWithViews();

                              switch (JsonSerializer)
                              {
                                  case JsonSerializerType.NewtonsoftJson:
                                      mvcBuilder.AddNewtonsoftJson(options =>
                                      {
                                          ConfigureNewtonsoftJson(context, options.SerializerSettings);

                                          foreach (var module in modules)
                                              module.ConfigureNewtonsoftJson(context, options.SerializerSettings);
                                      });

                                      break;
                                  case JsonSerializerType.SystemTextJson:
                                      mvcBuilder.AddJsonOptions(options =>
                                      {
                                          ConfigureSystemTextJson(context, options.JsonSerializerOptions);

                                          foreach (var module in modules)
                                              module.ConfigureSystemTextJson(context, options.JsonSerializerOptions);
                                      });

                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              ConfigureMvcBuilder(context, mvcBuilder);

                              foreach (var module in modules)
                                  module.ConfigureMvcBuilder(context, mvcBuilder);

                              // Configure swagger.
                              collection.AddSwaggerGen(options =>
                              {
                                  options.OperationFilter<OperationNameFilter>();

                                  options.CustomSchemaIds(x =>
                                  {
                                      if (x.FullName.Contains('+'))
                                          return x.FullName.Substring(x.FullName.LastIndexOf('.') + 1)
                                                  .Replace("+", "");

                                      return x.Name;
                                  });

                                  options.DocumentFilter<DocumentFilter>(hostIdentifier, hostVersion);

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

                              if (JsonSerializer == JsonSerializerType.NewtonsoftJson)
                                  collection.AddSwaggerGenNewtonsoftSupport();

                              // Configure cors.
                              collection.AddCors(options =>
                              {
                                  ConfigureCors(context, options);

                                  foreach (var module in modules)
                                      module.ConfigureCors(context, options);
                              });

                              // Configure migrations.
                              if (UseMigrations)
                              {
                                  var migrationsBuilder = collection.AddMigrations(context.Configuration);

                                  ConfigureMigrations(context, migrationsBuilder);

                                  foreach (var module in modules)
                                      module.ConfigureMigrations(context, migrationsBuilder);
                              }

                              // Configure services.
                              collection.ScanAssemblyForImplementations(typeof(DefaultWebHost).Assembly);
                              collection.ScanAssemblyForOptions(typeof(DefaultWebHost).Assembly, context.Configuration);

                              ConfigureServices(context, collection);

                              foreach (var module in modules)
                                  module.ConfigureServices(context, collection);

                              if (ValidateServices)
                                  ValidateServicesIfArePossibleToInstantiate(collection);
                          })
                          .Configure((context, applicationBuilder) =>
                          {
                              context            = context.AsNotNull();
                              applicationBuilder = applicationBuilder.AsNotNull();

                              if (UseHttpsRedirection)
                                  applicationBuilder.UseHttpsRedirection();

                              applicationBuilder.Use(async (context2, next) =>
                              {
                                  var logger = context2.RequestServices.GetRequiredService<ILogger<DefaultWebHost>>();

                                  var httpAbstractionOptions = context2.RequestServices.GetRequiredService<IOptions<DecoupleIt.HttpAbstraction.Options>>()
                                                                       .Value;

                                  context2.Response.OnStarting(() =>
                                  {
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostIdentifierHeaderName, HostInformation.Identifier.ToString());
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostNameHeaderName, HostInformation.Name);
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostVersionHeaderName, HostInformation.Version);

                                      return Task.CompletedTask;
                                  });

                                  using (logger.BeginScope(new SelfDescribingDictionary<string, object>
                                  {
                                      {
                                          "HostIdentifier", HostInformation.Identifier
                                      },
                                      {
                                          "HostName", HostInformation.Name
                                      },
                                      {
                                          "HostVersion", HostInformation.Version
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

                              applicationBuilder.UseRouting();

                              applicationBuilder.UseCors(builder =>
                              {
                                  builder = builder.AsNotNull();

                                  ConfigureCorsPolicyBuilder(context, builder);
                              });

                              applicationBuilder.UseTracing();

                              applicationBuilder.UseMiddleware<LoggingMiddleware>();

                              applicationBuilder.MaintainStorageOfContextualUnitOfWork();

                              applicationBuilder.UseOperations();

                              ConfigureApplication(context, applicationBuilder);

                              foreach (var module in modules)
                                  module.ConfigureApplication(context, applicationBuilder);

                              applicationBuilder.UseEndpoints(builder =>
                              {
                                  builder = builder.AsNotNull();

                                  builder.MapControllers();

                                  ConfigureEndpoints(context, builder);

                                  foreach (var module in modules)
                                      module.ConfigureEndpoints(context, builder);
                              });

                              if (UseMigrations)
                                  applicationBuilder.ExecuteMigrations();

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
