using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Scope
{
    /// <summary>
    ///     Delegate handling event emission.
    /// </summary>
    /// <param name="scope">Scope.</param>
    /// <param name="event">Event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    [NotNull]
    public delegate Task EventEmittedAsyncDelegate([NotNull] IInternalEventsScope scope, [NotNull] Event @event, CancellationToken cancellationToken);
}
