using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GS.DecoupleIt.AspNetCore.Service
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    internal sealed class DocumentFilter : IDocumentFilter
    {
        public DocumentFilter([NotNull] string hostVersion)
        {
            _hostVersion = hostVersion;
        }

        public void Apply([NotNull] OpenApiDocument document, [NotNull] DocumentFilterContext context)
        {
            document.Info.Description = $"HostVersion: {_hostVersion}";
        }

        [NotNull]
        private readonly string _hostVersion;
    }
}
