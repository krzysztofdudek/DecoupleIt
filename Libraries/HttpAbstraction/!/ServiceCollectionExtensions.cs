using System;
using System.Net.Http;
using GS.DecoupleIt.HttpAbstraction.Exceptions;
using GS.DecoupleIt.Options.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        ///     Adds http client service for specified interface. Implementation is created by RestEase.
        /// </summary>
        /// <param name="serviceCollection">Services collection.</param>
        /// <typeparam name="TService">Service type.</typeparam>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection AddHttpClientService<TService>([NotNull] this IServiceCollection serviceCollection)
            where TService : class
        {
            ContractGuard.IfArgumentIsNull(nameof(serviceCollection), serviceCollection);

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

                var requester = new Requester(httpClient, options, tracer);

                var implementation = ImplementationBuilder.Instance.CreateImplementation<TService>(requester);

                return implementation;
            });

            return serviceCollection;
        }

        /// <summary>
        ///     Configures http clients.
        /// </summary>
        /// <param name="serviceCollection">Services collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection ConfigureHttpClients([NotNull] this IServiceCollection serviceCollection, [NotNull] IConfiguration configuration)
        {
            var @namespace = typeof(ServiceCollectionExtensions).Namespace;

            var servicesUrisSectionKey = $"{@namespace}.ServicesUris".Replace('.', ':');

            var servicesUrisSection = configuration.GetSection(servicesUrisSectionKey)
                                                   .AsNotNull();

            serviceCollection.ScanAssemblyForOptions(typeof(ServiceCollectionExtensions).Assembly, configuration);
            serviceCollection.ConfigureDictionary<ServicesUrisOptions>(servicesUrisSection);

            return serviceCollection;
        }

        private sealed class WebRequestHandler : HttpClientHandler
        {
            public WebRequestHandler([NotNull] HttpAbstractionOptions settings)
            {
                if (settings.SkipSSLCertificateValidation)
                    ServerCertificateCustomValidationCallback = (
                        message,
                        certificate2,
                        arg3,
                        arg4) => true;
            }
        }
    }
}
