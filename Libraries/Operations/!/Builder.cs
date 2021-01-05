using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Builder for operations.
    /// </summary>
    [PublicAPI]
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }
    }
}
