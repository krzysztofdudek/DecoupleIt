using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IOnFailureInternalEventHandler
    {
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            HandleAsync([NotNull] IInternalEvent @event, [NotNull] Exception exception, CancellationToken cancellationToken = default);
    }
}
