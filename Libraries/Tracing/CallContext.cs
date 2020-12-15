using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Tracing
{
    [PublicAPI]
    internal static class CallContext
    {
        public static void RunInParallel([NotNull] Action action)
        {
            var executionContext = ExecutionContext.Capture()
                                                   ?.CreateCopy();

            if (executionContext != null)
                ExecutionContext.Run(executionContext, _ => action(), null);
            else
                action();
        }

        public static async Task RunInParallelAsync([NotNull] Func<Task> action)
        {
            var executionContext = ExecutionContext.Capture()
                                                   ?.CreateCopy();

            if (executionContext != null)
                ExecutionContext.Run(executionContext,
                                     _ => (action() ?? throw new Exception("Action cannot return null task.")).GetAwaiter()
                                         .GetResult(),
                                     null);
            else
                await (action() ?? throw new Exception("Action cannot return null task."));
        }
    }
}
