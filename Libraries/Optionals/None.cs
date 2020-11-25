using System;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    [PublicAPI]
    public sealed class None : IEquatable<None>
    {
        /// <summary>
        ///     Const value of nothing.
        /// </summary>
        [NotNull]
        public static None Value { get; } = new None();

        public override bool Equals(object obj)
        {
            return !(obj is null) && (obj is None || IsGenericNone(obj.GetType()));
        }

        public bool Equals(None other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        [NotNull]
        public override string ToString()
        {
            return "None";
        }

        private None() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private bool IsGenericNone([NotNull] Type type)
        {
            return type.GenericTypeArguments.Length == 1 && typeof(None<>).MakeGenericType(type.GenericTypeArguments[0]) == type;
        }
    }

    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    public sealed class None<T> : Optional<T>, IEquatable<None<T>>, IEquatable<None>
    {
        /// <summary>
        ///     Const value of nothing.
        /// </summary>
        [NotNull]
        public static None<T> Value { get; } = new None<T>();

        public static bool operator ==(None<T> a, None<T> b)
        {
            return a is null && b is null || !(a is null) && a.Equals(b);
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

        public override void Do(DoDelegate doAction) { }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync(DoAsyncDelegate doAction, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.CompletedTask!;
#else
            return new ValueTask();
#endif
        }

        public override bool Equals(object obj)
        {
            return !(obj is null) && (obj is None<T> || obj is None);
        }

        public bool Equals(None<T> other)
        {
            return true;
        }

        public bool Equals(None other)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override Optional<TResult> Map<TResult>(MapDelegate<TResult> map)
        {
            return None.Value;
        }

        public override Optional<TResult> Map<TResult>(MapWithNoParamDelegate<TResult> map)
        {
            return None.Value;
        }

        public override Optional<TResult> Map<TResult>(MapOptionalDelegate<TResult> map)
        {
            return None.Value;
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return None<TResult>.Value;
        }

        public override T Reduce(T whenNone)
        {
            return whenNone;
        }

        public override T Reduce(ReduceDelegate whenNone)
        {
            return whenNone()
                .AsNotNull();
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync(ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default)
        {
            return whenNone(cancellationToken)
                .AsNotNull();
        }

        public override Optional<T> ReduceToAlternate(T whenNone)
        {
            return whenNone;
        }

        public override Optional<T> ReduceToAlternate(AlternateDelegate alternateWay)
        {
            return alternateWay()
                .AsNotNull();
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync(AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default)
        {
            return alternateWay(cancellationToken)
                .AsNotNull();
        }

        public override T ReduceToDefault()
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
