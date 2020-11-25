using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExceptionCausingEventOnFailureHandler : OnFailureEventHandlerBase<ExceptionCausingEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(ExceptionCausingEvent @event, Exception exception, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

            throw new Exception();
        }
    }
}
