using System.Net.Http;
using System.Threading.Tasks;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RestEase;

namespace GS.DecoupleIt.HttpAbstraction
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    internal sealed class Requester : RestEase.Implementation.Requester
    {
        public Requester([NotNull] HttpClient httpClient, [NotNull] HttpAbstractionOptions options, [NotNull] ILogger<Requester> logger) : base(httpClient)
        {
            _httpClient = httpClient;
            _options    = options;
            _logger     = logger;
        }

        [NotNull]
        [ItemNotNull]
        protected override async Task<HttpResponseMessage> SendRequestAsync([NotNull] IRequestInfo requestInfo, bool readBody)
        {
            var basePath = SubstitutePathParameters(requestInfo.BasePath, requestInfo) ?? string.Empty;
            var path     = SubstitutePathParameters(requestInfo.Path, requestInfo) ?? string.Empty;

            var message = new HttpRequestMessage
            {
                Method     = requestInfo.Method,
                RequestUri = ConstructUri(basePath, path, requestInfo),
                Content    = ConstructContent(requestInfo),
                Properties =
                {
                    {
                        RestClient.HttpRequestMessageRequestInfoPropertyKey, requestInfo
                    }
                }
            };

            foreach (var requestMessageProperty in requestInfo.HttpRequestMessageProperties)
                message.Properties.Add(requestMessageProperty.Key, requestMessageProperty.Value);

            ApplyHeaders(requestInfo, message);

            var completionOption = readBody ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;

            using var span = Tracer.OpenChildSpan($"{message.Method} {message.RequestUri}", SpanType.OutgoingRequest);
            span.AttachResource(_logger.BeginTracerSpan());

            message.Headers.Add(_options.TraceIdHeaderName, span.Descriptor.TraceId.ToString());
            message.Headers.Add(_options.SpanIdHeaderName, span.Descriptor.Id.ToString());
            message.Headers.Add(_options.ParentSpanIdHeaderName, span.Descriptor.ParentId?.ToString());
            message.Headers.Add(_options.SpanNameHeaderName, span.Descriptor.Name);

            var response = await _httpClient.SendAsync(message, completionOption, requestInfo.CancellationToken)
                                            .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && !requestInfo.AllowAnyStatusCode)
                throw await ApiException.CreateAsync(message, response)
                                        .ConfigureAwait(false);

            return response;
        }

        [NotNull]
        private readonly HttpClient _httpClient;

        [NotNull]
        private readonly ILogger<Requester> _logger;

        [NotNull]
        private readonly HttpAbstractionOptions _options;
    }
}
