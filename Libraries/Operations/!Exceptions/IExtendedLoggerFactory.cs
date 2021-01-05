using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations
{
    internal interface IExtendedLoggerFactory
    {
        [NotNull]
        ILogger<TCategory> Create<TCategory>();
    }
}
