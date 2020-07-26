using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExceptionCausingEventOnEmissionHandler : OnEmissionEventHandlerBase<ExceptionCausingEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public static bool IsEnabled { get; set; }

        public override Task HandleAsync(ExceptionCausingEvent @event, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
                return Task.CompletedTask.AsNotNull();

            HandlesCount++;

            throw new Exception();
        }
    }
}
