using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Delegate used by <see cref="IOperationContextScope" /> to indicate internal events emission.
    /// </summary>
    [NotNull]
    public delegate
#if NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        InternalEventEmittedAsyncDelegate([NotNull] InternalEvent @event, CancellationToken cancellationToken = default);
}
