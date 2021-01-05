using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IPreCommandWithResultHandler
    {
#if NETCOREAPP2_2 || NETSTANDARD2_0
        [NotNull]
        Task
#else
        ValueTask
#endif
            PreHandleAsync([NotNull] ICommandWithResult command, CancellationToken cancellationToken = default);
    }
}
