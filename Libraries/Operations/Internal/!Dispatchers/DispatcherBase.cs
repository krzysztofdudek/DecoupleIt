using System;
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
        protected readonly IServiceProvider ServiceProvider;

        [NotNull]
        protected readonly ITracer Tracer;

        protected DispatcherBase([NotNull] ILogger logger, [NotNull] ITracer tracer, [NotNull] IServiceProvider serviceProvider)
        {
            Logger          = logger;
            Tracer          = tracer;
            ServiceProvider = serviceProvider;
        }
    }
}
