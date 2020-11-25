using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.InternalEvents.Tests
{
    internal sealed class ExceptionCausingEventOnEmissionHandler : OnEmissionEventHandlerBase<ExceptionCausingEvent>
    {
        [PublicAPI]
        public static int HandlesCount { get; set; }

        public static bool IsEnabled { get; set; }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            HandleAsync(ExceptionCausingEvent @event, CancellationToken cancellationToken = default)
        {
            if (!IsEnabled)
#if NETCOREAPP2_2 || NETSTANDARD2_0
                return Task.CompletedTask!;
#else
                return new ValueTask();
#endif

            HandlesCount++;

            throw new Exception();
        }
    }
}
