using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Logger that allows to overwrite log levels depending on the class of emission or exception being caught. For example business error can be lowered to
    ///     information level.
    /// </summary>
    public interface IExtendedLoggerFactory
    {
        /// <summary>
        ///     Creates an instance of <see cref="ILogger{TCategoryName}" />.
        /// </summary>
        /// <typeparam name="TCategoryName">Category name.</typeparam>
        /// <returns>An instance of logger.</returns>
        [NotNull]
        ILogger<TCategoryName> Create<TCategoryName>();
    }
}
