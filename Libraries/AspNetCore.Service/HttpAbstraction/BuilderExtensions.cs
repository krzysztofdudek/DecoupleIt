using System.Text.Json;
using GS.DecoupleIt.HttpAbstraction;
using JetBrains.Annotations;
using Newtonsoft.Json;
using RestEase;

namespace GS.DecoupleIt.AspNetCore.Service.HttpAbstraction
{
    /// <summary>
    ///     Http abstraction builder for Asp Net Core.
    /// </summary>
    [PublicAPI]
    public static class BuilderExtensions
    {
        /// <summary>
        ///     Registers <see cref="Newtonsoft" /> serializer as a json request body serializer.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="jsonSerializerSettings">Json serializer settings.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder UseNewtonsoftJsonRequestBodySerializer(
            [NotNull] this Builder builder,
            [CanBeNull] JsonSerializerSettings jsonSerializerSettings = default)
        {
            var serializer = new JsonRequestBodySerializer
            {
                JsonSerializerSettings = jsonSerializerSettings
            };

            return builder.UseRequestBodySerializer(serializer);
        }

        /// <summary>
        ///     Registers <see cref="Newtonsoft" /> serializer as a json response deserializer.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="jsonSerializerSettings">Json serializer settings.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder UseNewtonsoftJsonResponseDeserializer(
            [NotNull] this Builder builder,
            [CanBeNull] JsonSerializerSettings jsonSerializerSettings = default)
        {
            var serializer = new JsonResponseDeserializer
            {
                JsonSerializerSettings = jsonSerializerSettings
            };

            return builder.UseResponseDeserializer(serializer);
        }

        /// <summary>
        ///     Registers System.Text.Json serializer as a json request body serializer.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="jsonSerializerOptions">Json serializer options.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder UseSystemTextJsonRequestBodySerializer(
            [NotNull] this Builder builder,
            [CanBeNull] JsonSerializerOptions jsonSerializerOptions = default)
        {
            var serializer = new SystemTextJsonRequestBodySerializer(jsonSerializerOptions);

            return builder.UseRequestBodySerializer(serializer);
        }

        /// <summary>
        ///     Registers System.Text.Json serializer as a json response deserializer.
        /// </summary>
        /// <param name="builder">Builder.</param>
        /// <param name="jsonSerializerOptions">Json serializer options.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public static Builder UseSystemTextJsonResponseDeserializer(
            [NotNull] this Builder builder,
            [CanBeNull] JsonSerializerOptions jsonSerializerOptions = default)
        {
            var serializer = new SystemTextJsonResponseDeserializer(jsonSerializerOptions);

            return builder.UseResponseDeserializer(serializer);
        }
    }
}
