using System;
using JetBrains.Annotations;

namespace Samples.Documents.Command.Contracts.Services.Dtos
{
    /// <summary>
    ///     Create document.
    /// </summary>
    [NotNull]
    public sealed class CreateDocumentDto
    {
        /// <summary>
        ///     Client's identifier.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        ///     Content.
        /// </summary>
        public string Content { get; set; }
    }
}
