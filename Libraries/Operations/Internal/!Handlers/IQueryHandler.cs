using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.DependencyInjection.Automatic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [RegisterManyTimes]
    internal interface IQueryHandler
    {
#if NETSTANDARD2_0
        [NotNull]
        [ItemCanBeNull]
        Task<object>
#else
        ValueTask<object>
#endif
            HandleAsync([NotNull] IQuery query, CancellationToken cancellationToken = default);
    }
}
