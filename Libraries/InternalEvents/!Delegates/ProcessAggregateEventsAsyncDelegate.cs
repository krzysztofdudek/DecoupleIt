using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Delegate used for processing aggregated events.
    /// </summary>
    /// <param name="events">Events.</param>
    /// <returns>Task.</returns>
    [NotNull]
    public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        ProcessAggregateEventsAsyncDelegate([NotNull] [ItemNotNull] IReadOnlyCollection<Event> events);
}
