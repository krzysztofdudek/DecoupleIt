using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Operations.Internal;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations
{
    /// <summary>
    ///     Base class for all internal events.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public abstract class InternalEvent : Operation, IInternalEvent
    {
        /// <summary>
        ///     Emits this event.
        /// </summary>
        public void Emit()
        {
            OperationDispatcher.DispatchInternalEventAsync(this, CancellationToken.None)
#if !(NETCOREAPP2_2 || NETSTANDARD2_0)
                               .AsTask()
#endif
                               .GetAwaiter()
                               .GetResult();
        }

        /// <summary>
        ///     Emits this event.
        /// </summary>
        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            EmitAsync(CancellationToken cancellationToken = default)
        {
            return OperationDispatcher.DispatchInternalEventAsync(this, CancellationToken.None);
        }
    }
}
