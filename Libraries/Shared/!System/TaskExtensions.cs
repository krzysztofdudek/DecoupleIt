using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class TaskExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_0
        [NotNull]
        public static Task AsValueTask([NotNull] this Task task) => task;
#else
        public static ValueTask AsValueTask([NotNull] this Task task) => new(task);
#endif
    }
}
