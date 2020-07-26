using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Samples.Clients.Command.Contracts.Services;
using Samples.Documents.Command.Contracts.Services;
using Samples.Documents.Command.Contracts.Services.Dtos;

#pragma warning disable 1591

namespace Samples.Documents.Command.Controllers
{
    /// <inheritdoc cref="IDocuments" />
    [Route("api/v1/documents")]
    [ApiExplorerSettings(GroupName = "v1")]
    public sealed class DocumentsController : ControllerBase, IDocuments
    {
        public DocumentsController([NotNull] IClients clients)
        {
            _clients = clients;
        }

        /// <inheritdoc />
        [HttpPost]
        public async Task<CreatedDocumentDto> Create([BindRequired] [FromBody] CreateDocumentDto dto)
        {
            var client = await _clients.Get(dto.ClientId);

            var document = new Document(dto.Content, client.Id, client.Name);

            Documents.Add(document);

            return new CreatedDocumentDto
            {
                Id         = document.Id,
                Content    = document.Content,
                ClientId   = document.ClientId,
                ClientName = document.ClientName
            };
        }

        /// <inheritdoc />
        [HttpGet]
        public Task<IEnumerable<DocumentDto>> GetAll()
        {
            return Task.FromResult(Documents.Select(document => new DocumentDto
            {
                Id         = document.Id,
                Content    = document.Content,
                ClientId   = document.ClientId,
                ClientName = document.ClientName
            }));
        }

        [NotNull]
        [ItemNotNull]
        private static readonly List<Document> Documents = new List<Document>();

        [NotNull]
        private readonly IClients _clients;

        private sealed class Document
        {
            public Guid ClientId { get; }

            public string ClientName { get; }

            public string Content { get; }

            public Guid Id { get; }

            public Document(string content, Guid clientId, string clientName)
            {
                Id         = Guid.NewGuid();
                Content    = content;
                ClientId   = clientId;
                ClientName = clientName;
            }
        }
    }
}
