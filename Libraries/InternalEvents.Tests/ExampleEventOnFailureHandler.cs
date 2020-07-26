using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExampleEventOnFailureHandler : OnFailureEventHandlerBase<ExampleEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public override Task HandleAsync(ExampleEvent @event, Exception exception, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

            return Task.CompletedTask.AsNotNull();
        }
    }
}
