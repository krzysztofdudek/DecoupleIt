using GS.DecoupleIt.Tracing;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace GS.DecoupleIt.Operations.Internal
{
    internal abstract class DispatcherBase
    {
        [NotNull]
        protected readonly ILogger Logger;

        [NotNull]
        protected readonly OperationHandlerFactory OperationHandlerFactory;

        [NotNull]
        protected readonly ITracer Tracer;

        protected DispatcherBase([NotNull] ILogger logger, [NotNull] OperationHandlerFactory operationHandlerFactory, [NotNull] ITracer tracer)
        {
            Logger                  = logger;
            OperationHandlerFactory = operationHandlerFactory;
            Tracer                  = tracer;
        }
    }
}
