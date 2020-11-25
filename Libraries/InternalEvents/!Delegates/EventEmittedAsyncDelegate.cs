using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Delegate handling event emission.
    /// </summary>
    /// <param name="scope">Scope.</param>
    /// <param name="event">Event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    [NotNull]
    public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        EventEmittedAsyncDelegate([NotNull] IInternalEventsScope scope, [NotNull] Event @event, CancellationToken cancellationToken);
}
