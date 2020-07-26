using System;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GS.DecoupleIt.AspNetCore.Service
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    internal sealed class DocumentFilter : IDocumentFilter
    {
        public DocumentFilter(Guid hostIdentifier, string hostVersion)
        {
            _hostIdentifier = hostIdentifier;
            _hostVersion    = hostVersion;
        }

        public void Apply([NotNull] OpenApiDocument document, [NotNull] DocumentFilterContext context)
        {
            document.Info.Description = $"HostIdentifier: {_hostIdentifier}, HostVersion: {_hostVersion}";
        }

        private readonly Guid _hostIdentifier;

        private readonly string _hostVersion;
    }
}
