using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IPostCommandWithResultHandler
    {
#if NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            PostHandleAsync(
                [NotNull] ICommandWithResult command,
                [CanBeNull] object result,
                [NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> internalEvents,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken = default);
    }
}
