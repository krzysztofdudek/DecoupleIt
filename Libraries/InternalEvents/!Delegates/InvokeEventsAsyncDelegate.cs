using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents
{
    /// <summary>
    ///     Delegate destined for emission of internal events.
    /// </summary>
    [NotNull]
    public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        InvokeEventsAsyncDelegate();
}
