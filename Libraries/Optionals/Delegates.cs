using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    public static class Delegates<T>
    {
        [NotNull]
        [ItemNotNull]
        public delegate
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            AlternateAsyncDelegate(CancellationToken cancellationToken);

        [NotNull]
        public delegate Optional<T> AlternateDelegate();

        [NotNull]
        public delegate
#if NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsyncDelegate([NotNull] T obj, CancellationToken cancellationToken);

        public delegate void DoDelegate([NotNull] T obj);

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            MapAsyncDelegate<TResult>([NotNull] T obj, CancellationToken cancellationToken);

        [NotNull]
        public delegate TResult MapDelegate<out TResult>([NotNull] T obj);

        [NotNull]
        public delegate
#if NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapOptionalAsyncDelegate<TResult>([NotNull] T obj, CancellationToken cancellationToken);

        [NotNull]
        public delegate Optional<TResult> MapOptionalDelegate<TResult>([NotNull] T obj);

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETSTANDARD2_0
            Task<TResult>
#else
            ValueTask<TResult>
#endif
            MapWithNoParamAsyncDelegate<TResult>(CancellationToken cancellationToken);

        [NotNull]
        public delegate TResult MapWithNoParamDelegate<out TResult>();

        [NotNull]
        [ItemNotNull]
        public delegate
#if NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsyncDelegate(CancellationToken cancellationToken);

        [NotNull]
        public delegate T ReduceDelegate();
    }
}
