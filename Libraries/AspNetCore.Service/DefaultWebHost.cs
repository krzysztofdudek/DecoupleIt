using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public override void ConfigureConfiguration(WebHostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddJsonFile("appsettings.json", true, true);
            configurationBuilder.AddJsonFile("appsettings.user.json", true, true);
            configurationBuilder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true);
            configurationBuilder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.user.json", true, true);
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public override void ConfigureLogging(WebHostBuilderContext context, LoggerConfiguration configuration)
        {
            configuration.MinimumLevel.Debug()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
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

            if (HostIdentifierLoggerProperty is not null)
                configuration.Enrich.WithProperty(HostIdentifierLoggerProperty, HostInformation.Identifier.ToString());

            if (HostNameLoggerProperty is not null)
                configuration.Enrich.WithProperty(HostNameLoggerProperty, HostInformation.Name);

            if (HostVersionLoggerProperty is not null)
                configuration.Enrich.WithProperty(HostVersionLoggerProperty, HostInformation.Version);

            if (EnvironmentLoggerProperty is not null)
                configuration.Enrich.WithProperty(EnvironmentLoggerProperty, HostInformation.Environment);

            var options = context.Configuration.GetValue<bool?>("GS:DecoupleIt:AspNetCore:Service:Logging:Console:Enabled");

            if (options != false)
                configuration.WriteTo.Console(
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

        /// <summary>
        ///     Configures <see cref="GS.DecoupleIt.Tracing.Builder" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="builder">Cors policy builder.</param>
        public virtual void ConfigureTracing([NotNull] WebHostBuilderContext context, [NotNull] GS.DecoupleIt.Tracing.Builder builder) { }

        /// <inheritdoc cref="WebHostExtensions.Run" />
        /// <param name="args">Arguments.</param>
        /// <param name="configureWebHostBuilder">Configure web host builder delegate.</param>
        public void Run([CanBeNull] [ItemCanBeNull] string[] args, [CanBeNull] Action<IWebHostBuilder> configureWebHostBuilder = default)
        {
            var webHostBuilder = ConfigureWebHostBuilder(args);

            configureWebHostBuilder?.Invoke(webHostBuilder);

            var webHost = webHostBuilder.Build();

            webHost!.Run();
        }

        /// <inheritdoc cref="IWebHost.Start" />
        /// <param name="args">Arguments.</param>
        /// <param name="configureWebHostBuilder">Configure web host builder delegate.</param>
        /// <returns>Web host.</returns>
        [NotNull]
        public IWebHost Start([CanBeNull] [ItemCanBeNull] string[] args, [CanBeNull] Action<IWebHostBuilder> configureWebHostBuilder = default)
        {
            var webHostBuilder = ConfigureWebHostBuilder(args);

            configureWebHostBuilder?.Invoke(webHostBuilder);

            var webHost = webHostBuilder.Build();

            webHost!.Start();

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
        ///     Environment logger property name. If null, property is not added. It's null by default.
        /// </summary>
        [CanBeNull]
        protected virtual string EnvironmentLoggerProperty { get; }

        /// <summary>
        ///     Host identifier logger property name. If null, property is not added. It's null by default.
        /// </summary>
        [CanBeNull]
        protected virtual string HostIdentifierLoggerProperty { get; }

        /// <summary>
        ///     Host name.
        /// </summary>
        protected virtual string HostName =>
            GetType()
                .Assembly.GetName()
                .Name;

        /// <summary>
        ///     Host name logger property name. If null, property is not added. It's null by default.
        /// </summary>
        [CanBeNull]
        protected virtual string HostNameLoggerProperty { get; }

        /// <summary>
        ///     Host version.
        /// </summary>
        protected virtual string HostVersion =>
            GetType()
                .Assembly.GetName()
                .Version?.ToString();

        /// <summary>
        ///     Host version logger property name. If null, property is not added. It's null by default.
        /// </summary>
        [CanBeNull]
        protected virtual string HostVersionLoggerProperty { get; }

        /// <summary>
        ///     Type of an json serializer used for whole pipeline.
        /// </summary>
        protected virtual JsonSerializerType JsonSerializer { get; } = JsonSerializerType.SystemTextJson;

        /// <summary>
        ///     If this flag is set, https is enforced. It's disabled by default.
        /// </summary>
        protected virtual bool UseHttpsRedirection { get; }

        /// <summary>
        ///     If this flag is set, jobs engine is enabled. It's enabled by default.
        /// </summary>
        protected virtual bool UseJobs { get; } = true;

        /// <summary>
        ///     If this flag is set, migration engine is enabled. It's enabled by default.
        /// </summary>
        protected virtual bool UseMigrations { get; } = true;

        /// <summary>
        ///     If this flag is set, then web host will test all registered services if are possible to instantiate.
        /// </summary>
        protected virtual bool ValidateServices { get; } = true;

        /// <summary>
        ///     Configures <see cref="ServiceProviderOptions" />.
        /// </summary>
        /// <param name="context">Web host builder context.</param>
        /// <param name="options">Service provider options.</param>
        protected virtual void ConfigureServiceProvider([NotNull] WebHostBuilderContext context, [NotNull] ServiceProviderOptions options)
        {
            options.ValidateScopes  = true;
            options.ValidateOnBuild = true;
        }

        /// <summary>
        ///     Gets modules.
        /// </summary>
        /// <returns>Modules.</returns>
        [NotNull]
        [ItemNotNull]
        protected virtual IReadOnlyCollection<IWebHostModule> GetModules()
        {
            return Array.Empty<IWebHostModule>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CognitiveComplexity")]
        [NotNull]
        private IWebHostBuilder ConfigureWebHostBuilder(string[] args)
        {
            var modules = GetModules()
                          .Concat(new[]
                          {
                              this
                          })
                          .ToList();

            static IReadOnlyCollection<Assembly> GetAssembliesFromTypeInheritanceStack([NotNull] Type type)
            {
                return type.GetAllBaseTypes()
                           .Reverse()
                           .Concat(new[]
                           {
                               type
                           })
                           .Select(x => x.Assembly)
                           .Distinct()
                           .ToList();
            }

            var allApplicationAssemblies = modules.Select(x => GetAssembliesFromTypeInheritanceStack(x.GetType()))
                                                  .SelectMany(x => x)
                                                  .Concat(GetAssembliesFromTypeInheritanceStack(GetType()))
                                                  .Distinct()
                                                  .ToList();

            var webHostBuilder = new WebHostBuilder();

            webHostBuilder.UseKestrel()
                          .UseContentRoot(Directory.GetCurrentDirectory())
                          .UseDefaultServiceProvider(ConfigureServiceProvider)
                          .ConfigureAppConfiguration((context, builder) =>
                          {
                              foreach (var module in modules)
                                  module.ConfigureConfiguration(context, builder);
                          })
                          .ConfigureServices((context, serviceCollection) =>
                          {
                              var hostIdentifier = Environment.GetEnvironmentVariable("ASPNETCORE_HOSTIDENTIFIER")
                                                              ?.ToLower() == "default"
                                  ? Guid.Empty
                                  : Guid.NewGuid();

                              var hostEnvironment = context.HostingEnvironment.EnvironmentName;

                              HostInformation = new HostInformation(hostIdentifier,
                                                                    HostName,
                                                                    HostVersion,
                                                                    hostEnvironment);

                              serviceCollection.Add(ServiceDescriptor.Singleton(typeof(IHostInformation), HostInformation));

                              // Configure automatic dependency injection.
                              serviceCollection.ConfigureAutomaticDependencyInjection(context.Configuration,
                                                                                      options =>
                                                                                      {
                                                                                          options.Environment = context.HostingEnvironment.EnvironmentName;
                                                                                      });

                              // Configure scheduling.
                              serviceCollection.ConfigureJobs(context.Configuration,
                                                              options =>
                                                              {
                                                                  ConfigureScheduling(context, options);

                                                                  foreach (var module in modules)
                                                                      module.ConfigureScheduling(context, options);
                                                              });

                              // Configure unit of work.
                              var unitOfWorkBuilder = serviceCollection.AddContextualUnitOfWork(context.Configuration.AsNotNull());

                              foreach (var module in modules)
                                  module.ConfigureUnitOfWork(context, unitOfWorkBuilder);

                              // Configure tracing.
                              var tracingBuilder = serviceCollection.AddTracingForAspNetCore(context.Configuration.AsNotNull());

                              ConfigureTracing(context, tracingBuilder);

                              // Configure operations.
                              var operationsBuilder = serviceCollection.AddOperationsForAspNetCore(context.Configuration.AsNotNull());

                              foreach (var module in modules)
                                  module.ConfigureOperations(context, operationsBuilder);

                              // Configure json serializer.
                              var jsonSerializerSettings = new JsonSerializerSettings();
                              var jsonSerializerOptions  = new JsonSerializerOptions();

                              switch (JsonSerializer)
                              {
                                  case JsonSerializerType.NewtonsoftJson:
                                      foreach (var module in modules)
                                          module.ConfigureNewtonsoftJson(context, jsonSerializerSettings);

                                      break;
                                  case JsonSerializerType.SystemTextJson:
                                      foreach (var module in modules)
                                          module.ConfigureSystemTextJson(context, jsonSerializerOptions);

                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              // Configure http clients.
                              var httpClientBuilder = serviceCollection.AddHttpClients(context.Configuration.AsNotNull());

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
                              var mvcBuilder = serviceCollection.AddControllersWithViews();

                              switch (JsonSerializer)
                              {
                                  case JsonSerializerType.NewtonsoftJson:
                                      mvcBuilder.AddNewtonsoftJson(options =>
                                      {
                                          foreach (var module in modules)
                                              module.ConfigureNewtonsoftJson(context, options.SerializerSettings);
                                      });

                                      break;
                                  case JsonSerializerType.SystemTextJson:
                                      mvcBuilder.AddJsonOptions(options =>
                                      {
                                          foreach (var module in modules)
                                              module.ConfigureSystemTextJson(context, options.JsonSerializerOptions);
                                      });

                                      break;
                                  default:
                                      throw new ArgumentOutOfRangeException();
                              }

                              foreach (var module in modules)
                                  module.ConfigureMvcBuilder(context, mvcBuilder);

                              foreach (var assembly in allApplicationAssemblies)
                                  mvcBuilder.AddApplicationPart(assembly);

                              // Configure swagger.
                              serviceCollection.AddSwaggerGen(options =>
                              {
                                  options.OperationFilter<OperationNameFilter>();

                                  options.DocumentFilter<DocumentFilter>(HostInformation.Version);

                                  foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(GetType()
                                                                                                      .Assembly.Location)
                                                                                    .AsNotNull())
                                                                .AsNotNull()
                                                                .Where(x => x.ToLower()
                                                                             .TrimEnd()
                                                                             .EndsWith(".xml")))
                                      options.IncludeXmlComments(file);

                                  foreach (var module in modules)
                                      module.ConfigureSwaggerGen(context, options);
                              });

                              if (JsonSerializer == JsonSerializerType.NewtonsoftJson)
                                  serviceCollection.AddSwaggerGenNewtonsoftSupport();

                              // Configure cors.
                              serviceCollection.AddCors(options =>
                              {
                                  foreach (var module in modules)
                                      module.ConfigureCors(context, options);
                              });

                              // Configure routing.
                              serviceCollection.AddRouting(options =>
                              {
                                  foreach (var module in modules)
                                      module.ConfigureRoute(context, options);
                              });

                              // Configure migrations.
                              if (UseMigrations)
                              {
                                  var migrationsBuilder = serviceCollection.AddMigrations(context.Configuration);

                                  foreach (var module in modules)
                                      module.ConfigureMigrations(context, migrationsBuilder);
                              }

                              var thisAssembly = typeof(DefaultWebHost).Assembly;

                              serviceCollection.ScanAssemblyForImplementations(thisAssembly);
                              serviceCollection.ScanAssemblyForOptions(thisAssembly, context.Configuration.AsNotNull());
                              serviceCollection.ScanAssemblyForJobs(thisAssembly);
                              serviceCollection.ScanAssemblyForHttpClients(thisAssembly);

                              // Configure services.
                              foreach (var module in modules)
                                  module.ConfigureServices(context, serviceCollection);

                              if (ValidateServices)
                                  ValidateServicesIfArePossibleToInstantiate(serviceCollection);
                          })
                          .Configure((context, applicationBuilder) =>
                          {
                              context            = context.AsNotNull();
                              applicationBuilder = applicationBuilder.AsNotNull();

                              var tracer = applicationBuilder.ApplicationServices.GetRequiredService<ITracer>()
                                                             .AsNotNull();

                              using var tracerSpan = tracer.OpenSpan(GetType(), SpanType.InternalProcess);

                              // Get options.
                              var options = applicationBuilder.ApplicationServices.GetRequiredService<IOptions<Options>>()
                                                              .Value;

                              // Log information about running application.
                              var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>()
                                                             .CreateLogger(GetType())
                                                             .AsNotNull();

                              // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                              logger.LogInformation(
                                  "Host started.\nIdentifier: {@HostIdentifier}\nName: {HostName}\nVersion: {HostVersion}\nEnvironment: {HostEnvironment}",
                                  HostInformation.Identifier,
                                  HostInformation.Name,
                                  HostInformation.Version,
                                  HostInformation.Environment);

                              // Perform configuration of application.
                              if (UseHttpsRedirection)
                                  applicationBuilder.UseHttpsRedirection();

                              foreach (var module in modules)
                                  module.PreConfigureApplication(context, applicationBuilder);

                              applicationBuilder.UseSwagger(swaggerOptions =>
                              {
                                  swaggerOptions = swaggerOptions.AsNotNull();

                                  foreach (var module in modules)
                                      module.ConfigureSwagger(context, swaggerOptions);
                              });

                              applicationBuilder.UseSwaggerUI(swaggerUIOptions =>
                              {
                                  swaggerUIOptions = swaggerUIOptions.AsNotNull();

                                  swaggerUIOptions.RoutePrefix = "swagger";

                                  foreach (var module in modules)
                                      module.ConfigureSwaggerUI(context, swaggerUIOptions);
                              });

                              applicationBuilder.UseRouting();

                              applicationBuilder.UseCors(corsPolicyBuilder =>
                              {
                                  corsPolicyBuilder = corsPolicyBuilder.AsNotNull();

                                  foreach (var module in modules)
                                      module.ConfigureCorsPolicyBuilder(context, corsPolicyBuilder);
                              });

                              applicationBuilder.Use((context2, next) =>
                              {
                                  var httpAbstractionOptions = context2.RequestServices.GetRequiredService<IOptions<DecoupleIt.HttpAbstraction.Options>>()
                                                                       .Value;

                                  context2.Response.OnStarting(() =>
                                  {
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostIdentifierHeaderName, HostInformation.Identifier.ToString());
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostNameHeaderName, HostInformation.Name);
                                      context2.Response.Headers.Add(httpAbstractionOptions.HostVersionHeaderName, HostInformation.Version);

                                      return Task.CompletedTask;
                                  });

                                  return next();
                              });

                              applicationBuilder.UseTracing();

                              if (options.Logging.Enabled)
                                  applicationBuilder.UseMiddleware<LoggingMiddleware>();

                              applicationBuilder.MaintainStorageOfContextualUnitOfWork();

                              applicationBuilder.UseOperations();

                              foreach (var module in modules)
                                  module.ConfigureApplication(context, applicationBuilder);

                              applicationBuilder.UseEndpoints(endpointRouteBuilder =>
                              {
                                  endpointRouteBuilder = endpointRouteBuilder.AsNotNull();

                                  endpointRouteBuilder.MapControllers();

                                  foreach (var module in modules)
                                      module.ConfigureEndpoints(context, endpointRouteBuilder);
                              });

                              if (UseMigrations)
                                  applicationBuilder.ExecuteMigrations();

                              if (UseJobs)
                                  applicationBuilder.UseDefaultJobScheduling();
                          })
                          .UseSerilog((context, configuration) =>
                          {
                              foreach (var module in modules)
                                  module.ConfigureLogging(context, configuration);
                          });

            ConfigureWebHostBuilder(webHostBuilder);

            foreach (var module in modules)
                module.ConfigureWebHostBuilder(webHostBuilder);

            return webHostBuilder;
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
