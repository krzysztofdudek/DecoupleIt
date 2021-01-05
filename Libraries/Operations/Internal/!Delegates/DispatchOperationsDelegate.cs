using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Operations.Internal
{
    [NotNull]
    public delegate
#if NETCOREAPP2_2 || NETSTANDARD2_0
        Task
#else
        ValueTask
#endif
        DispatchOperationsDelegate();
}
