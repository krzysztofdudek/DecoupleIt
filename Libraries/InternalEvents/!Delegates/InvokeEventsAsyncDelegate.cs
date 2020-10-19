using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Delegate destined for emission of internal events.
    /// </summary>
    [NotNull]
    public delegate Task InvokeEventsAsyncDelegate();
}
