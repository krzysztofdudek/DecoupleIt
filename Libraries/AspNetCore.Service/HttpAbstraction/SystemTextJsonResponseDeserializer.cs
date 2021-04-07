using System.Net.Http;
using System.Text.Json;
using JetBrains.Annotations;
using RestEase;

// ReSharper disable once EmptyNamespace
namespace GS.DecoupleIt.AspNetCore.Service.HttpAbstraction
{
    internal sealed class SystemTextJsonResponseDeserializer : ResponseDeserializer
    {
        public SystemTextJsonResponseDeserializer([CanBeNull] JsonSerializerOptions options)
        {
            _options = options;
        }

        public override T Deserialize<T>(string content, HttpResponseMessage response, ResponseDeserializerInfo info)
        {
            return string.IsNullOrWhiteSpace(content) ? default : JsonSerializer.Deserialize<T>(content, _options);
        }

        [CanBeNull]
        private readonly JsonSerializerOptions _options;
    }
}
