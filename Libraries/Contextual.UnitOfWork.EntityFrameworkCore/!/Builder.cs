using GS.DecoupleIt.Shared;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GS.DecoupleIt.Contextual.UnitOfWork.EntityFrameworkCore
{
    /// <summary>
    ///     Builder class of unit of work extensions for Entity Framework Core.
    /// </summary>
    [PublicAPI]
    public sealed class Builder : ExtensionBuilderBase
    {
        internal Builder([NotNull] IServiceCollection serviceCollection, [NotNull] IConfiguration configuration) : base(serviceCollection, configuration) { }
    }
}
