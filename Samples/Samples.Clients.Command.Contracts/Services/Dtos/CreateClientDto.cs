using JetBrains.Annotations;

namespace Samples.Clients.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Dto used to create client.
    /// </summary>
    [PublicAPI]
    public sealed class CreateClientDto
    {
        /// <summary>
        ///     Name.
        /// </summary>
        public string Name { get; set; }
    }
}
