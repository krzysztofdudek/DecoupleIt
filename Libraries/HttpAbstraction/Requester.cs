using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using RestEase;

namespace GS.DecoupleIt.HttpAbstraction
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "LogMessageIsSentenceProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantNullCoalescingCondition")]
    internal sealed class Requester : RestEase.Implementation.Requester
    {
        public Requester(
            [NotNull] HttpClient httpClient,
            [NotNull] Options options,
            [NotNull] ITracer tracer,
            [NotNull] ILogger<Requester> logger) : base(httpClient)
        {
            _httpClient = httpClient;
            _options    = options;
            _tracer     = tracer;
            _logger     = logger;
        }

        [NotNull]
        [ItemNotNull]
        protected override async Task<HttpResponseMessage> SendRequestAsync([NotNull] IRequestInfo requestInfo, bool readBody)
        {
            var baseAddress = SubstitutePathParameters(requestInfo.BaseAddress, requestInfo) ?? string.Empty;
            var basePath    = SubstitutePathParameters(requestInfo.BasePath, requestInfo) ?? string.Empty;
            var path        = SubstitutePathParameters(requestInfo.Path, requestInfo) ?? string.Empty;

            var message = new HttpRequestMessage
            {
                Method = requestInfo.Method,
                RequestUri = ConstructUri(baseAddress,
                                          basePath,
                                          path,
                                          requestInfo),
                Content = ConstructContent(requestInfo),
#if !NET5_0
                Properties =
                {
                    {
                        RestClient.HttpRequestMessageRequestInfoPropertyKey, requestInfo
                    }
                }
#endif
            };
#if NET5_0
            foreach (var requestMessageProperty in requestInfo.HttpRequestMessageProperties)
                message.Options.TryAdd(requestMessageProperty.Key, requestMessageProperty.Value);
#else
            foreach (var requestMessageProperty in requestInfo.HttpRequestMessageProperties)
                message.Properties.Add(requestMessageProperty.Key, requestMessageProperty.Value);
#endif

            ApplyHeaders(requestInfo, message);

            var completionOption = readBody ? HttpCompletionOption.ResponseContentRead : HttpCompletionOption.ResponseHeadersRead;

            using var span = _tracer.OpenSpan($"{message.Method} {message.RequestUri}", SpanType.OutgoingRequest);

            message.Headers.Add(_options.TraceIdHeaderName, span.Descriptor.TraceId.ToString());
            message.Headers.Add(_options.SpanIdHeaderName, span.Descriptor.Id.ToString());
            message.Headers.Add(_options.ParentSpanIdHeaderName, span.Descriptor.ParentId?.ToString());
            message.Headers.Add(_options.SpanNameHeaderName, span.Descriptor.Name);

            await LogStart(message);

            var stopwatch = Stopwatch.StartNew();

            var response = await _httpClient.SendAsync(message, completionOption, requestInfo.CancellationToken)
                                            .ConfigureAwait(false);

            stopwatch.Stop();

            await LogFinish(response!, stopwatch.Elapsed);

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
        private readonly Options _options;

        [NotNull]
        private readonly ITracer _tracer;

        private async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            LogFinish([NotNull] HttpResponseMessage context, TimeSpan duration)
        {
            var message = new StringBuilder(
                "Outgoing request handling {@OperationAction} after {@OperationDuration}ms.\nStatus code: {@HttpStatusCode}\nHeaders: {@OperationMetadata}");

            var args = new List<object>
            {
                "finished",
                (int) duration.TotalMilliseconds,
                (int) context.StatusCode,
                context.Headers?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, IEnumerable<string>>()
            };

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            var responseBody = context.Content is not null ? await context.Content.ReadAsStringAsync() : null;

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                message.Append("\nBody: {@Body:l}");
                args.Add(responseBody);
            }

            _logger.LogInformation(message.ToString(), args.ToArray());
        }

        private async
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            LogStart([NotNull] HttpRequestMessage context)
        {
            var message = new StringBuilder(
                "Outgoing request handling {@OperationAction}.\nMethod: {@HttpMethod}\nPath: {@HttpPath}\nHeaders: {@OperationMetadata}");

            var args = new List<object>
            {
                "started",
                context.Method.Method,
                context.RequestUri,
                context.Headers?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, IEnumerable<string>>()
            };

            var requestBody = context.Content is not null ? await context.Content.ReadAsStringAsync() : null;

            if (!string.IsNullOrWhiteSpace(requestBody))
            {
                message.Append("\nBody: {@Body:l}");
                args.Add(requestBody);
            }

            _logger.LogInformation(message.ToString(), args.ToArray());
        }
    }
}
