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
        public TracingMiddleware([NotNull] IOptions<HeadersOptions> options, [NotNull] ILogger<TracingMiddleware> logger)
        {
            _logger  = logger;
            _options = options.Value.AsNotNull();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            Tracer.Initialize();

            try
            {
                var request        = context.Request.AsNotNull();
                var requestHeaders = request.Headers.AsNotNull();
                var traceIds       = requestHeaders.TryGetValue(_options.TraceIdHeaderName);
                var traceId        = traceIds.Count == 1 ? traceIds[0] : null;
                var spanIds        = requestHeaders.TryGetValue(_options.SpanIdHeaderName);
                var spanId         = spanIds.Count == 1 ? spanIds[0] : null;
                var spanNames      = requestHeaders.TryGetValue(_options.SpanNameHeaderName);
                var spanName       = spanNames.Count == 1 ? spanNames[0] : null;
                var parentSpanIds  = requestHeaders.TryGetValue(_options.ParentSpanIdHeaderName);
                var parentSpanId   = parentSpanIds.Count == 1 ? parentSpanIds[0] : null;

                if (traceId is null || spanId is null)
                    traceId = spanId = Tracer.NewTracingIdGenerator();

                spanName ??= string.Empty;

                context.Response.AsNotNull()
                       .OnStarting(httpContextObject =>
                                   {
                                       var httpContext = (HttpContext) httpContextObject;

                                       var responseHeaders = httpContext.AsNotNull()
                                                                        .Response.AsNotNull()
                                                                        .Headers.AsNotNull();

                                       responseHeaders.Add(_options.TraceIdHeaderName, traceId);
                                       responseHeaders.Add(_options.SpanIdHeaderName, spanId);
                                       responseHeaders.Add(_options.SpanNameHeaderName, spanName);
                                       responseHeaders.Add(_options.ParentSpanIdHeaderName, parentSpanId);

                                       return Task.CompletedTask;
                                   },
                                   context);

                using var scope = Tracer.OpenRootSpan(traceId,
                                                      spanId,
                                                      spanName,
                                                      parentSpanId,
                                                      SpanType.ExternalRequest);

                scope.AttachResource(_logger.BeginTracerSpan());

                await next(context)
                    .AsNotNull();
            }
            finally
            {
                Tracer.Clear();
            }
        }

        [NotNull]
        private readonly ILogger<TracingMiddleware> _logger;

        [NotNull]
        private readonly HeadersOptions _options;
    }
}
