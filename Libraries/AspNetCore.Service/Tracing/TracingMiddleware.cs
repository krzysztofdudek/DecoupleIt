using System.Collections.Generic;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using GS.DecoupleIt.Shared;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace GS.DecoupleIt.AspNetCore.Service.Tracing
{
    [Transient]
    internal sealed class TracingMiddleware : IMiddleware
    {
        public TracingMiddleware(
            [NotNull] IOptions<HeadersOptions> headersOptions,
            [NotNull] IOptions<global::GS.DecoupleIt.Tracing.Options> options,
            [NotNull] ITracer tracer)
        {
            _options        = options.Value!;
            _tracer         = tracer;
            _headersOptions = headersOptions.Value!;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
        public async Task InvokeAsync([NotNull] HttpContext context, [NotNull] RequestDelegate next)
        {
            var request        = context.Request.AsNotNull();
            var requestHeaders = request.Headers.AsNotNull();
            var traceIds       = requestHeaders.TryGetValue(_headersOptions.TraceIdHeaderName);
            var traceId        = traceIds.Count == 1 ? traceIds[0] : null;
            var spanIds        = requestHeaders.TryGetValue(_headersOptions.SpanIdHeaderName);
            var spanId         = spanIds.Count == 1 ? spanIds[0] : null;
            var spanNames      = requestHeaders.TryGetValue(_headersOptions.SpanNameHeaderName);
            var spanName       = spanNames.Count == 1 ? spanNames[0] : null;
            var parentSpanIds  = requestHeaders.TryGetValue(_headersOptions.ParentSpanIdHeaderName);
            var parentSpanId   = parentSpanIds.Count == 1 ? parentSpanIds[0] : null;

            spanId  ??= _options.NewTracingIdGenerator();
            traceId ??= spanId;

            context.Response.AsNotNull()
                   .OnStarting(httpContextObject =>
                               {
                                   var httpContext = (HttpContext) httpContextObject;

                                   var responseHeaders = httpContext.AsNotNull()
                                                                    .Response.AsNotNull()
                                                                    .Headers.AsNotNull();

                                   AddOrReplace(responseHeaders, _headersOptions.TraceIdHeaderName, traceId);
                                   AddOrReplace(responseHeaders, _headersOptions.SpanIdHeaderName, spanId);
                                   AddOrReplace(responseHeaders, _headersOptions.SpanNameHeaderName, spanName);
                                   AddOrReplace(responseHeaders, _headersOptions.ParentSpanIdHeaderName, parentSpanId);

                                   return Task.CompletedTask;
                               },
                               context);

            using var scope = _tracer.OpenSpan(new TracingId(traceId),
                                               new TracingId(spanId),
                                               spanName ?? "unknown",
                                               parentSpanId is null ? null : new TracingId(parentSpanId),
                                               SpanType.ExternalRequest);

            await next(context)
                .AsNotNull();
        }

        private static void AddOrReplace([NotNull] IHeaderDictionary headerDictionary, [NotNull] string key, StringValues stringValues)
        {
            if (headerDictionary.ContainsKey(key))
                headerDictionary.Remove(key);

            headerDictionary.Add(key, stringValues);
        }

        [NotNull]
        private readonly HeadersOptions _headersOptions;

        [NotNull]
        private readonly DecoupleIt.Tracing.Options _options;

        [NotNull]
        private readonly ITracer _tracer;
    }
}
