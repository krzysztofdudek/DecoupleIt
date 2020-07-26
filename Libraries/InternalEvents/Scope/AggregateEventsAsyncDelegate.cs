using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Scope
{
    /// <summary>
    ///     Delegate used for run operations emitting events.
    /// </summary>
    [NotNull]
    public delegate Task AggregateEventsAsyncDelegate();
}
