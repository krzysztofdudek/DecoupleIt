using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [NotNull]
    internal delegate
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        InternalEventEmittedAsyncDelegate([NotNull] InternalEvent @event, CancellationToken cancellationToken);
}
