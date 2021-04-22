using System;
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
        ///     Indicates if event was already emitted.
        /// </summary>
        public bool WasEmitted { get; private set; }

        /// <summary>
        ///     Emits this event.
        /// </summary>
        public void Emit()
        {
            if (WasEmitted)
                throw new EventWasAlreadyEmitted();

            WasEmitted = true;

            OperationDispatcher.DispatchInternalEventAsync(this, CancellationToken.None)
#if !NETSTANDARD2_0
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
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            EmitAsync(CancellationToken cancellationToken = default)
        {
            if (WasEmitted)
                throw new EventWasAlreadyEmitted();

            WasEmitted = true;

            return OperationDispatcher.DispatchInternalEventAsync(this, CancellationToken.None);
        }

        /// <summary>
        ///     Event thrown, when an instance of event was already emitted and next attempt of emission occurs.
        /// </summary>
        public sealed class EventWasAlreadyEmitted : Exception { }
    }
}
