using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GS.DecoupleIt.Tracing.AspNetCore
{
    [Transient]
    internal sealed class TracingMiddleware : IMiddleware
    {
        public TracingMiddleware([NotNull] IOptions<TracingOptions> options, [NotNull] ILogger<TracingMiddleware> logger)
        {
            _logger  = logger;
            _options = options.Value.AsNotNull();
        }

        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            Tracer.Initialize();

            try
            {
                var request        = context.Request.AsNotNull();
                var requestHeaders = request.Headers.AsNotNull();

                var traceIds = requestHeaders.TryGetValue(_options.Headers.TraceIdHeaderName);
                var spanIds  = requestHeaders.TryGetValue(_options.Headers.SpanIdHeaderName);

                var spanName = requestHeaders.TryGetValue(_options.Headers.SpanNameHeaderName)
                                             .ToString() ?? "undefined";

                var parentSpanIds = requestHeaders.TryGetValue(_options.Headers.ParentSpanIdHeaderName);

                Guid traceId, spanId, parentSpanId;

                if (traceIds.Count == 1 && spanIds.Count == 1)
                {
                    traceId = traceIds[0]
                        .IfNotNull(Guid.Parse);

                    spanId = spanIds[0]
                        .IfNotNull(Guid.Parse);

                    if (parentSpanIds.Count == 1)
                        parentSpanId = parentSpanIds[0]
                            .IfNotNull(Guid.Parse);
                    else
                        parentSpanId = default;
                }
                else
                {
                    traceId      = spanId = Guid.NewGuid();
                    parentSpanId = default;
                }

                context.Response.AsNotNull()
                       .OnStarting(httpContextObject =>
                                   {
                                       var httpContext = (HttpContext) httpContextObject;

                                       var responseHeaders = httpContext.AsNotNull()
                                                                        .Response.AsNotNull()
                                                                        .Headers.AsNotNull();

                                       responseHeaders.Add(_options.Headers.TraceIdHeaderName, traceId.ToString());
                                       responseHeaders.Add(_options.Headers.SpanIdHeaderName, spanId.ToString());
                                       responseHeaders.Add(_options.Headers.SpanNameHeaderName, spanName);
                                       responseHeaders.Add(_options.Headers.ParentSpanIdHeaderName, parentSpanId.ToString());

                                       return Task.CompletedTask;
                                   },
                                   context);

                using (var scope = Tracer.OpenRootSpan(traceId,
                                                       spanId,
                                                       spanName,
                                                       parentSpanId,
                                                       SpanType.ExternalRequest))
                {
                    scope.AttachResource(_logger.BeginTracerSpan());

                    await next(context)
                        .AsNotNull();
                }
            }
            finally
            {
                Tracer.Clear();
            }
        }

        [NotNull]
        private readonly ILogger<TracingMiddleware> _logger;

        [NotNull]
        private readonly TracingOptions _options;
    }
}
