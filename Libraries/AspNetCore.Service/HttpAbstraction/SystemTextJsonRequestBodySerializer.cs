using System.Net.Http;
using System.Text.Json;
using JetBrains.Annotations;
using RestEase;

// ReSharper disable once EmptyNamespace
namespace GS.DecoupleIt.AspNetCore.Service.HttpAbstraction
{
    internal sealed class SystemTextJsonRequestBodySerializer : RequestBodySerializer
    {
        public SystemTextJsonRequestBodySerializer([CanBeNull] JsonSerializerOptions options)
        {
            _options = options;
        }

        public override HttpContent SerializeBody<T>(T body, RequestBodySerializerInfo info)
        {
            if (body == null)
                return null;

            var stringContent = new StringContent(JsonSerializer.Serialize(body, _options));
            stringContent.Headers!.ContentType!.MediaType = "application/json";

            return stringContent;
        }

        [CanBeNull]
        private readonly JsonSerializerOptions _options;
    }
}
