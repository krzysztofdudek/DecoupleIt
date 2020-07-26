using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RestEase;
using Samples.Documents.Command.Contracts.Services.Dtos;

namespace Samples.Documents.Command.Contracts.Services
{
    /// <summary>
    ///     Service allows to manage documents.
    /// </summary>
    [PublicAPI]
    [BasePath("api/v1/documents")]
    public interface IDocuments
    {
        /// <summary>
        ///     Creates document.
        /// </summary>
        /// <param name="dto">Data.</param>
        /// <returns>Created document.</returns>
        [NotNull]
        [ItemNotNull]
        [Post]
        Task<CreatedDocumentDto> Create([NotNull] [Body] CreateDocumentDto dto);

        /// <summary>
        ///     Gets all documents.
        /// </summary>
        /// <returns>Documents.</returns>
        [NotNull]
        [ItemNotNull]
        [Get]
        Task<IEnumerable<DocumentDto>> GetAll();
    }
}
