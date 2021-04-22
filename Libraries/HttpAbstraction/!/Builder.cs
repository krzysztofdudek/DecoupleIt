using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestEase;

namespace GS.DecoupleIt.HttpAbstraction
{
    /// <summary>
    ///     Http abstraction builder.
    /// </summary>
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }

        /// <summary>
        ///     Registers service for request body serializer.
        /// </summary>
        /// <param name="requestBodySerializer">Instance.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public Builder UseRequestBodySerializer([NotNull] RequestBodySerializer requestBodySerializer)
        {
            ContractGuard.IfArgumentIsNull(nameof(requestBodySerializer), requestBodySerializer);

            ServiceCollection.AddSingleton(requestBodySerializer);

            return this;
        }

        /// <summary>
        ///     Registers service for response deserializer.
        /// </summary>
        /// ć
        /// <param name="responseDeserializer">Instance.</param>
        /// <returns>Builder.</returns>
        [NotNull]
        public Builder UseResponseDeserializer([NotNull] ResponseDeserializer responseDeserializer)
        {
            ContractGuard.IfArgumentIsNull(nameof(responseDeserializer), responseDeserializer);

            ServiceCollection.AddSingleton(responseDeserializer);

            return this;
        }
    }
}
