using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IOnEmissionInternalEventHandler
    {
#if NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            HandleAsync([NotNull] IInternalEvent @event, CancellationToken cancellationToken = default);
    }
}
