using System;
using JetBrains.Annotations;

namespace Samples.Documents.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Document.
    /// </summary>
    [PublicAPI]
    public sealed class DocumentDto
    {
        /// <summary>
        ///     Client's identifier.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        ///     Client's name.
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        ///     Content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     Identifier.
        /// </summary>
        public Guid Id { get; set; }
    }
}
