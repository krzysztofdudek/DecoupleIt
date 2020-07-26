using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestEase;

namespace GS.DecoupleIt.HttpAbstraction
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyInHierarchy")]
    internal sealed class Requester : RestEase.Implementation.Requester
    {
        public Requester([NotNull] HttpClient httpClient) : base(httpClient)
        {
            _httpClient = httpClient;
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

            var response = await _httpClient.SendAsync(message, completionOption, requestInfo.CancellationToken)
                                            .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && !requestInfo.AllowAnyStatusCode)
                throw await ApiException.CreateAsync(message, response)
                                        .ConfigureAwait(false);

            return response;
        }

        [NotNull]
        private readonly HttpClient _httpClient;
    }
}
