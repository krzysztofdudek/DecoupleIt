using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Scope
{
    /// <summary>
    ///     Delegate used for processing aggregated events.
    /// </summary>
    /// <param name="events">Events.</param>
    public delegate void ProcessAggregateEventsDelegate([NotNull] [ItemNotNull] IReadOnlyCollection<Event> events);
}
