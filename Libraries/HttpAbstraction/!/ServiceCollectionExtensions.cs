using System;
using System.Net.Http;
using GS.DecoupleIt.HttpAbstraction.Exceptions;
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
                var options = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>()
                                             .AsNotNull()
                                             .Value.AsNotNull();

                var tracingOptions = serviceProvider.GetRequiredService<IOptions<TracingOptions>>()
                                                    .AsNotNull()
                                                    .Value.AsNotNull();

                var servicesUrisOptions = serviceProvider.GetRequiredService<IOptions<ServicesUrisOptions>>()
                                                         .AsNotNull()
                                                         .Value.AsNotNull();

                var serviceAssemblyName = typeof(TService).Assembly.GetName()
                                                          .Name.AsNotNull();

                var serviceName = serviceAssemblyName.EndsWith(".Contracts")
                    ? serviceAssemblyName.Substring(0, serviceAssemblyName.LastIndexOf(".", StringComparison.Ordinal))
                    : serviceAssemblyName;

                if (!servicesUrisOptions.TryGetValue(serviceName, out var boundedContextUriString))
                    throw new MissingServiceUriMapping(serviceName);

                var currentTracerScope = Tracer.CurrentSpan;

                var httpClient = new HttpClient(new WebRequestHandler(options))
                {
                    BaseAddress = new Uri(boundedContextUriString.AsNotNull()),
                    Timeout     = TimeSpan.FromMilliseconds(options.TimeoutMs),
                    DefaultRequestHeaders =
                    {
                        {
                            tracingOptions.Headers.TraceIdHeaderName, currentTracerScope.TraceId.ToString()
                        },
                        {
                            tracingOptions.Headers.SpanIdHeaderName, currentTracerScope.Id.ToString()
                        },
                        {
                            tracingOptions.Headers.ParentSpanIdHeaderName, currentTracerScope.ParentId?.ToString()
                        },
                        {
                            tracingOptions.Headers.SpanNameHeaderName, currentTracerScope.Name
                        },
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

                var requester = new Requester(httpClient);

                return ImplementationBuilder.Instance.CreateImplementation<TService>(requester);
            });

            return serviceCollection;
        }

        /// <summary>
        ///     Configures http clients.
        /// </summary>
        /// <param name="serviceCollection">Services collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="configureHttpClientOptions">Configure <see cref="HttpClientOptions" /> method.</param>
        /// <param name="configureServicesUrisOptions">Configure <see cref="ServicesUrisOptions" /> method.</param>
        /// <returns>Service collection.</returns>
        [NotNull]
        public static IServiceCollection ConfigureHttpClients(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] IConfiguration configuration,
            [CanBeNull] ConfigureDelegate<HttpClientOptions> configureHttpClientOptions = default,
            [CanBeNull] ConfigureDelegate<ServicesUrisOptions> configureServicesUrisOptions = default)
        {
            serviceCollection.ConfigureOptionsAndPostConfigure(configuration, configureHttpClientOptions);
            serviceCollection.ConfigureOptionsDictionaryAndPostConfigure(configuration, configureServicesUrisOptions);

            return serviceCollection;
        }

        private sealed class WebRequestHandler : HttpClientHandler
        {
            public WebRequestHandler([NotNull] HttpClientOptions settings)
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
