using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GS.DecoupleIt.AspNetCore.Service.Tracing;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace GS.DecoupleIt.AspNetCore.Service.Tests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class TracingMiddlewareTests
    {
        private const string Url = "http://localhost:60020";
        private const string TestEndpoint = "test";
        private static Guid _traceId = Guid.NewGuid();
        private static Guid _spanId = Guid.NewGuid();
        private static Guid _parentSpanId = Guid.NewGuid();
        private static readonly string _spanName = nameof(TracingMiddlewareTests);

        [Fact]
        public async Task TestHeaders()
        {
            Guid   traceId      = default, spanId = default;
            Guid?  parentSpanId = default;
            string spanName     = default;

            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureWebHost(webHostBuilder =>
                       {
                           webHostBuilder.UseKestrel()
                                         .UseUrls(Url)
                                         .Configure(applicationBuilder =>
                                         {
                                             applicationBuilder = applicationBuilder.AsNotNull();

                                             applicationBuilder.UseTracing();

                                             applicationBuilder.Use((context, _) =>
                                             {
                                                 var tracer = context.RequestServices.GetService<ITracer>();

                                                 traceId      = tracer.CurrentSpan!.Descriptor.TraceId;
                                                 spanId       = tracer.CurrentSpan!.Descriptor.Id;
                                                 spanName     = tracer.CurrentSpan!.Descriptor.Name;
                                                 parentSpanId = tracer.CurrentSpan!.Descriptor.ParentId;

                                                 return Task.CompletedTask;
                                             });
                                         })
                                         .UseDefaultServiceProvider((context, options) =>
                                         {
                                             var isDevelopment = context.AsNotNull()
                                                                        .HostingEnvironment.IsDevelopment();

                                             options.AsNotNull()
                                                    .ValidateScopes = isDevelopment;

                                             options.AsNotNull()
                                                    .ValidateOnBuild = isDevelopment;
                                         });
                       })
                       .ConfigureServices((context, collection) =>
                       {
                           collection = collection.AsNotNull();

                           collection.ConfigureAutomaticDependencyInjection(context.Configuration!);

                           collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());
                       });

            var host = hostBuilder.Build()
                                  .AsNotNull();

            await host.StartAsync()
                      .AsNotNull();

            var tracingOptions = host.Services.AsNotNull()
                                     .GetRequiredService<IOptions<HeadersOptions>>()
                                     .AsNotNull()
                                     .Value;

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(Url)
            };

            var headers = httpClient.DefaultRequestHeaders.AsNotNull();
            headers.Add(tracingOptions.TraceIdHeaderName, _traceId.ToString());
            headers.Add(tracingOptions.SpanIdHeaderName, _spanId.ToString());
            headers.Add(tracingOptions.SpanNameHeaderName, _spanName);
            headers.Add(tracingOptions.ParentSpanIdHeaderName, _parentSpanId.ToString());

            var response = await httpClient.GetAsync(TestEndpoint)
                                           .AsNotNull();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseHeaders = response.AsNotNull()
                                          .Headers.AsNotNull();

            Assert.Equal(responseHeaders.GetValues(tracingOptions.TraceIdHeaderName)
                                        .AsNotNull()
                                        .Single(),
                         traceId.ToString());

            Assert.Equal(responseHeaders.GetValues(tracingOptions.SpanIdHeaderName)
                                        .AsNotNull()
                                        .Single(),
                         spanId.ToString());

            Assert.Equal(responseHeaders.GetValues(tracingOptions.SpanNameHeaderName)
                                        .AsNotNull()
                                        .Single(),
                         spanName);

            Assert.Equal(responseHeaders.GetValues(tracingOptions.ParentSpanIdHeaderName)
                                        .AsNotNull()
                                        .Single(),
                         parentSpanId?.ToString());

            Assert.Equal(_traceId, traceId);
            Assert.Equal(_spanId, spanId);
            Assert.Equal(_spanName, spanName);
            Assert.Equal(_parentSpanId, parentSpanId);

            await host.StopAsync();
            host.Dispose();
        }
    }
}
