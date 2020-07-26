using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExampleEventOnEmissionHandler : OnEmissionEventHandlerBase<ExampleEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public override Task HandleAsync(ExampleEvent @event, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

            return Task.CompletedTask.AsNotNull();
        }
    }
}
