using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using GS.DecoupleIt.HttpAbstraction.Exceptions;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestEase;
using RestEase.Implementation;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Extends <see cref="IServiceCollection" />.
    /// </summary>
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds support for http clients automatic implementation. <see cref="RestEase" /> runs this feature.
        /// </summary>
        /// <param name="serviceCollection">Service collection</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder AddHttpClients([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);
            ContractGuard.IfArgumentIsNull(nameof(configuration), configuration);

            var @namespace = typeof(ServiceCollectionExtensions).Namespace;

            var servicesUrisSectionKey = $"{@namespace}.ServicesUris".Replace('.', ':');

            var servicesUrisSection = configuration.GetSection(servicesUrisSectionKey)
                                                   .AsNotNull();

            serviceCollection.ScanAssemblyForOptions(typeof(ServiceCollectionExtensions).Assembly, configuration);
            serviceCollection.ConfigureDictionary<ServicesUrisOptions>(servicesUrisSection);

            return new Builder(serviceCollection, configuration);
        }

        public static void ScanAssemblyForHttpClients([NotNull] this IServiceCollection serviceCollection, [NotNull] Assembly assembly)
        {
            var httpClientInterfaces = assembly.GetTypes()
                                               .Where(x => x.GetCustomAttributes<HttpClientAttribute>()
                                                            .Any() && x.IsInterface)
                                               .ToList();

            foreach (var @interface in httpClientInterfaces)
                typeof(ServiceCollectionExtensions).GetMethod(nameof(For), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(@interface!)
                    .Invoke(null,
                            new object[]
                            {
                                serviceCollection
                            });
        }

        private static void For<TService>([NotNull] IServiceCollection serviceCollection)
            where TService : class
        {
            serviceCollection.AddTransient(serviceProvider =>
            {
                serviceProvider = serviceProvider.AsNotNull();

                var options = serviceProvider.GetRequiredService<IOptions<HttpAbstractionOptions>>()
                                             .AsNotNull()
                                             .Value.AsNotNull();

                var servicesUrisOptions = serviceProvider.GetRequiredService<IOptions<ServicesUrisOptions>>()
                                                         .AsNotNull()
                                                         .Value.AsNotNull();

                var serviceAssemblyName = typeof(TService).Assembly.GetName()
                                                          .Name.AsNotNull();

                var tracer = serviceProvider.GetRequiredService<ITracer>()
                                            .AsNotNull();

                var logger = serviceProvider.GetRequiredService<ILogger<Requester>>()
                                            .AsNotNull();

                var serviceName = serviceAssemblyName.EndsWith(".Contracts")
                    ? serviceAssemblyName.Substring(0, serviceAssemblyName.LastIndexOf(".", StringComparison.Ordinal))
                    : serviceAssemblyName;

                if (!servicesUrisOptions.TryGetValue(serviceName, out var uri))
                    throw new MissingServiceUriMapping(serviceName);

                var httpClient = new HttpClient(new WebRequestHandler(options))
                {
                    BaseAddress = new Uri(uri.AsNotNull()),
                    Timeout     = TimeSpan.FromMilliseconds(options.TimeoutMs),
                    DefaultRequestHeaders =
                    {
                        {
                            options.HostIdentifierHeaderName, options.HostIdentifier.ToString()
                        },
                        {
                            options.HostNameHeaderName, options.HostName
                        },
                        {
                            options.HostVersionHeaderName, options.HostVersion
                        }
                    }
                };

                var requester = new Requester(httpClient,
                                              options,
                                              tracer,
                                              logger);

                var requestBodySerializer = serviceProvider.GetService<RequestBodySerializer>();
                var responseDeserializer  = serviceProvider.GetService<ResponseDeserializer>();

                if (requestBodySerializer != null)
                    requester.RequestBodySerializer = requestBodySerializer;

                if (responseDeserializer != null)
                    requester.ResponseDeserializer = responseDeserializer;

                var implementation = ImplementationBuilder.Instance.CreateImplementation<TService>(requester);

                return implementation;
            });
        }

        private sealed class WebRequestHandler : HttpClientHandler
        {
            public WebRequestHandler([NotNull] HttpAbstractionOptions settings)
            {
                if (settings.SkipSSLCertificateValidation)
                    ServerCertificateCustomValidationCallback = (
                        _,
                        _,
                        _,
                        _) => true;
            }
        }
    }
}
