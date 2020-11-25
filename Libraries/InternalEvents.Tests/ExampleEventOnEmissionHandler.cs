using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExampleEventOnEmissionHandler : OnEmissionEventHandlerBase<ExampleEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(ExampleEvent @event, CancellationToken cancellationToken = default)
        {
            HandlesCount++;

#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask!;
#else
            return new ValueTask();
#endif
        }
    }
}
