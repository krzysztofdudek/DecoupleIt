using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
    public readonly struct None<T> : IEquatable<None<T>>
    {
        /// <summary>
        ///     Const value of nothing.
        /// </summary>
        [NotNull]
        public static None<T> Value { get; } = new None<T>();

        public static bool operator ==(None<T> a, None<T> b)
        {
            return a.Equals(b);
        }

        [NotNull]
        public static implicit operator
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ([NotNull] None<T> none)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult<Optional<T>>(none)!;
#else
            return new ValueTask<Optional<T>>(none);
#endif
        }

        public static bool operator !=(None<T> a, None<T> b)
        {
            return !(a == b);
        }

        public void Do(Delegates<T>.DoDelegate doAction) { }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync(Delegates<T>.DoAsyncDelegate doAction, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask!;
#else
            return new ValueTask();
#endif
        }

        public override bool Equals(object obj)
        {
            return obj is None<T>;
        }

        public bool Equals(None<T> other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public Optional<TResult> Map<TResult>(Delegates<T>.MapDelegate<TResult> map)
        {
            return None<TResult>.Value;
        }

        public Optional<TResult> Map<TResult>(Delegates<T>.MapWithNoParamDelegate<TResult> map)
        {
            return None<TResult>.Value;
        }

        public Optional<TResult> Map<TResult>(Delegates<T>.MapOptionalDelegate<TResult> map)
        {
            return None<TResult>.Value;
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(Delegates<T>.MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(Delegates<T>.MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        public T Reduce(T whenNone)
        {
            return whenNone;
        }

        [NotNull]
        public T Reduce([NotNull] Delegates<T>.ReduceDelegate whenNone)
        {
            return whenNone()
                .AsNotNull();
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync([NotNull] Delegates<T>.ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default)
        {
            return whenNone(cancellationToken)
                .AsNotNull();
        }

        public Optional<T> ReduceToAlternate(T whenNone)
        {
            return whenNone;
        }

        public Optional<T> ReduceToAlternate([NotNull] Delegates<T>.AlternateDelegate alternateWay)
        {
            return alternateWay()
                .AsNotNull();
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync([NotNull] Delegates<T>.AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default)
        {
            return alternateWay(cancellationToken)
                .AsNotNull();
        }

        public T ReduceToDefault()
        {
            return default;
        }

        [NotNull]
        public override string ToString()
        {
            return "None";
        }
    }
}
