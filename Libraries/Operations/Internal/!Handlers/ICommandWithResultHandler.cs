using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface ICommandWithResultHandler
    {
#if NETSTANDARD2_0
        [NotNull]
        [ItemCanBeNull]
        Task<object>
#else
        ValueTask<object>
#endif
            HandleAsync([NotNull] ICommandWithResult command, CancellationToken cancellationToken = default);
    }
}
