using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

#if NETCOREAPP3_1
using Microsoft.Extensions.Hosting;

#endif

namespace GS.DecoupleIt.Tracing.AspNetCore.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class TracingMiddlewareTests
    {
        private const string Url = "http://localhost:60020";
        private const string TestEndpoint = "test";
        private static Guid _traceId = Guid.NewGuid();
        private static Guid _spanId = Guid.NewGuid();
        private static Guid _parentSpanId = Guid.NewGuid();
        private static readonly string _spanName = nameof(TracingMiddlewareTests);

        [Test]
        public async Task TestHeaders()
        {
            Guid   traceId      = default, spanId = default;
            Guid?  parentSpanId = default;
            string spanName     = default;

#if NETCOREAPP3_1
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureWebHost(webHostBuilder =>
                       {
                           webHostBuilder.UseKestrel()
                                         .UseUrls(Url)
                                         .Configure(applicationBuilder =>
                                         {
                                             applicationBuilder = applicationBuilder.AsNotNull();

                                             applicationBuilder.UseTracing();

                                             applicationBuilder.Use((context, func) =>
                                             {
                                                 traceId = Tracer.CurrentSpan.TraceId;
                                                 spanId = Tracer.CurrentSpan.Id;
                                                 spanName = Tracer.CurrentSpan.Name;
                                                 parentSpanId = Tracer.CurrentSpan.ParentId;

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

                           collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());
                       });
#elif NETCOREAPP2_2
            var hostBuilder = new WebHostBuilder();

            hostBuilder.UseKestrel()
                       .UseUrls(Url)
                       .Configure(applicationBuilder =>
                       {
                           applicationBuilder = applicationBuilder.AsNotNull();

                           applicationBuilder.UseTracing();

                           applicationBuilder.Use((context, func) =>
                           {
                               traceId      = Tracer.CurrentSpan.TraceId;
                               spanId       = Tracer.CurrentSpan.Id;
                               spanName     = Tracer.CurrentSpan.Name;
                               parentSpanId = Tracer.CurrentSpan.ParentId;

                               return Task.CompletedTask;
                           });
                       })
                       .UseDefaultServiceProvider((context, options) =>
                       {
                           var isDevelopment = context.AsNotNull()
                                                      .HostingEnvironment.IsDevelopment();

                           options.AsNotNull()
                                  .ValidateScopes = isDevelopment;
                       })
                       .AsNotNull()
                       .ConfigureServices((context, collection) =>
                       {
                           collection = collection.AsNotNull();

                           collection.AddTracingForAspNetCore(context.Configuration.AsNotNull());

                           collection.AddLogging();
                       });
#endif

            var host = hostBuilder.Build()
                                  .AsNotNull();

            await host.StartAsync()
                      .AsNotNull();

            var tracingOptions = host.Services.GetRequiredService<IOptions<TracingOptions>>()
                                     .AsNotNull()
                                     .Value.AsNotNull()
                                     .Headers;

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

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseHeaders = response.AsNotNull()
                                          .Headers.AsNotNull();

            Assert.AreEqual(responseHeaders.GetValues(tracingOptions.TraceIdHeaderName)
                                           .AsNotNull()
                                           .Single(),
                            traceId.ToString());

            Assert.AreEqual(responseHeaders.GetValues(tracingOptions.SpanIdHeaderName)
                                           .AsNotNull()
                                           .Single(),
                            spanId.ToString());

            Assert.AreEqual(responseHeaders.GetValues(tracingOptions.SpanNameHeaderName)
                                           .AsNotNull()
                                           .Single(),
                            spanName);

            Assert.AreEqual(responseHeaders.GetValues(tracingOptions.ParentSpanIdHeaderName)
                                           .AsNotNull()
                                           .Single(),
                            parentSpanId?.ToString());

            Assert.AreEqual(_traceId, traceId);
            Assert.AreEqual(_spanId, spanId);
            Assert.AreEqual(_spanName, spanName);
            Assert.AreEqual(_parentSpanId, parentSpanId);
        }
    }
}