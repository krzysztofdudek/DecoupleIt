using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IPostCommandHandler
    {
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            PostHandleAsync(
                [NotNull] ICommand command,
                [NotNull] [ItemNotNull] IReadOnlyCollection<InternalEvent> internalEvents,
                [CanBeNull] Exception exception,
                CancellationToken cancellationToken = default);
    }
}
