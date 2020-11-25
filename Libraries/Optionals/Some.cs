using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    [PublicAPI]
    public sealed class Some<T> : Optional<T>, IEquatable<Some<T>>
    {
        public static bool operator ==(Some<T> a, Some<T> b)
        {
            return a is null && b is null || !(a is null) && a.Equals(b);
        }

        [NotNull]
        public static implicit operator T([NotNull] Some<T> some)
        {
            return some.Content;
        }

        [NotNull]
        public static implicit operator Some<T>([NotNull] T value)
        {
            return new Some<T>(value);
        }

        public static bool operator !=(Some<T> a, Some<T> b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Content of optional.
        /// </summary>
        [NotNull]
        public T Content { get; }

        public Some([NotNull] T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Content = value;
        }

        public override void Do(DoDelegate doAction)
        {
            doAction(Content);
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync(DoAsyncDelegate doAction, CancellationToken cancellationToken = default)
        {
            return doAction(Content, cancellationToken)
                .AsNotNull();
        }

        public bool Equals(Some<T> other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return EqualityComparer<T>.Default.Equals(Content, other.Content);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is Some<T> some && Equals(some);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Content);
        }

        public override Optional<TResult> Map<TResult>(MapDelegate<TResult> map)
        {
            return map(Content)
                .AsNotNull();
        }

        public override Optional<TResult> Map<TResult>(MapWithNoParamDelegate<TResult> map)
        {
            return map()
                .AsNotNull();
        }

        public override Optional<TResult> Map<TResult>(MapOptionalDelegate<TResult> map)
        {
            return map(Content)
                .AsNotNull();
        }

        public override async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return await map(Content, cancellationToken)
                .AsNotNull();
        }

        public override async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return await map(cancellationToken)
                .AsNotNull();
        }

        public override async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>(MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return (await map(Content, cancellationToken)
                .AsNotNull()).AsNotNull();
        }

        public override T Reduce(T whenNone)
        {
            return Content;
        }

        public override T Reduce(ReduceDelegate whenNone)
        {
            return Content;
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync(ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult(Content)!;
#else
            return new ValueTask<T>(Content);
#endif
        }

        public override Optional<T> ReduceToAlternate(T whenNone)
        {
            return Content;
        }

        public override Optional<T> ReduceToAlternate(AlternateDelegate alternateWay)
        {
            return Content;
        }

        public override
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync(AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult<Optional<T>>(Content);
#else
            return new ValueTask<Optional<T>>(Content);
#endif
        }

        [NotNull]
        public override T ReduceToDefault()
        {
            return Content;
        }

        [NotNull]
        public override string ToString()
        {
            return $"Some({ContentToString})";
        }

        [NotNull]
        private string ContentToString =>
            Content.ToString()
                   .AsNotNull();
    }
}
