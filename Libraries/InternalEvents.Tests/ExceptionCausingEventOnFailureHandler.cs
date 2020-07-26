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

        public override Task HandleAsync(ExceptionCausingEvent @event, Exception exception, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

            throw new Exception();
        }
    }
}
