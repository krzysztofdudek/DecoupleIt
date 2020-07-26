using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExceptionCausingEventOnSuccessHandler : OnSuccessEventHandlerBase<ExceptionCausingEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public override Task HandleAsync(ExceptionCausingEvent @event, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

            throw new Exception();
        }
    }
}
