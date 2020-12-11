using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "CA1067")]
#pragma warning disable 660,661
    public readonly struct Some<T> : IEquatable<Some<T>>
#pragma warning restore 660,661
    {
        public static bool operator ==(Some<T> a, Some<T> b)
        {
            return a.Equals(b);
        }

        [NotNull]
        public static implicit operator T(Some<T> some)
        {
            return some.Content;
        }

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

        public void Do([NotNull] Delegates<T>.DoDelegate doAction)
        {
            doAction(Content);
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task
#else
            ValueTask
#endif
            DoAsync([NotNull] Delegates<T>.DoAsyncDelegate doAction, CancellationToken cancellationToken = default)
        {
            return doAction(Content, cancellationToken)
                .AsNotNull();
        }

        public bool Equals(Some<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Content, other.Content);
        }

        public new bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Some<T> some && Equals(some);
        }

        public new int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Content);
        }

        public Optional<TResult> Map<TResult>([NotNull] Delegates<T>.MapDelegate<TResult> map)
        {
            return map(Content)
                .AsNotNull();
        }

        public Optional<TResult> Map<TResult>([NotNull] Delegates<T>.MapWithNoParamDelegate<TResult> map)
        {
            return map()
                .AsNotNull();
        }

        public Optional<TResult> Map<TResult>([NotNull] Delegates<T>.MapOptionalDelegate<TResult> map)
        {
            return map(Content)
                .AsNotNull();
        }

        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([NotNull] Delegates<T>.MapAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return await map(Content, cancellationToken)
                .AsNotNull();
        }

        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([NotNull] Delegates<T>.MapWithNoParamAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return await map(cancellationToken)
                .AsNotNull();
        }

        public async
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<TResult>>
#else
            ValueTask<Optional<TResult>>
#endif
            MapAsync<TResult>([NotNull] Delegates<T>.MapOptionalAsyncDelegate<TResult> map, CancellationToken cancellationToken = default)
        {
            return (await map(Content, cancellationToken)
                .AsNotNull()).AsNotNull();
        }

        [NotNull]
        public T Reduce(T whenNone)
        {
            return Content;
        }

        [NotNull]
        public T Reduce(Delegates<T>.ReduceDelegate whenNone)
        {
            return Content;
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<T>
#else
            ValueTask<T>
#endif
            ReduceAsync(Delegates<T>.ReduceAsyncDelegate whenNone, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult(Content)!;
#else
            return new ValueTask<T>(Content);
#endif
        }

        public Optional<T> ReduceToAlternate(T whenNone)
        {
            return Content;
        }

        public Optional<T> ReduceToAlternate(Delegates<T>.AlternateDelegate alternateWay)
        {
            return Content;
        }

        [NotNull]
        public
#if NETCOREAPP2_2 || NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            ReduceToAlternateAsync(Delegates<T>.AlternateAsyncDelegate alternateWay, CancellationToken cancellationToken = default)
        {
#if NETCOREAPP2_2 || NETSTANDARD2_0
            return Task.FromResult<Optional<T>>(Content);
#else
            return new ValueTask<Optional<T>>(Content);
#endif
        }

        [NotNull]
        public T ReduceToDefault()
        {
            return Content;
        }

        [NotNull]
        public new string ToString()
        {
            return $"Some({ContentToString})";
        }

        [NotNull]
        private string ContentToString =>
            Content.ToString()
                   .AsNotNull();
    }
}
