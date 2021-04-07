using System.Threading;
using System.Threading.Tasks;
using GS.DecoupleIt.Shared;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    /// <summary>
    ///     Extends all objects with optional extension methods.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AnnotationRedundancyAtValueType")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Treat <paramref name="obj" /> as optional.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public static Optional<T> AsOptional<T>([CanBeNull] this T obj)
        {
            return obj.When(!ReferenceEquals(obj, null));
        }

        /// <summary>
        ///     Tries to cast <paramref name="obj" /> to <typeparamref name="T" />.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Optional.</returns>
        [NotNull]
        public static Optional<T> TryCast<T>([CanBeNull] this object obj)
        {
            if (obj is T typed)
                return typed;

            return new Optional<T>();
        }

        /// <summary>
        ///     Transforms <paramref name="obj" /> to optional if condition is equal true.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="condition">Condition.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Conditional.</returns>
        [NotNull]
        public static Optional<T> When<T>([CanBeNull] this T obj, bool condition)
        {
            return condition ? new Optional<T>(obj) : new Optional<T>();
        }

        /// <summary>
        ///     Transforms <paramref name="obj" /> to optional if predicate is equal true.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="predicate">Predicate.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Conditional.</returns>
        [NotNull]
        public static Optional<T> When<T>([CanBeNull] this T obj, [NotNull] WhenDelegate<T> predicate)
        {
            return obj.When(predicate(obj));
        }

        /// <summary>
        ///     Transforms <paramref name="obj" /> to optional if condition is equal true.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="condition">Condition.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Conditional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            WhenAsync<T>([CanBeNull] this Task<T> obj, bool condition)
        {
            if (obj is null)
                return new Optional<T>();

            if (!condition)
                return new Optional<T>();

            return (await obj).AsOptional();
        }

        /// <summary>
        ///     Transforms <paramref name="obj" /> to optional if predicate is equal true.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="predicate">Predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Conditional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            WhenAsync<T>([CanBeNull] this T obj, [InstantHandle] [NotNull] WhenAsyncDelegate<T> predicate, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(predicate), predicate);

            return obj.When(await predicate(obj, cancellationToken));
        }

        /// <summary>
        ///     Transforms <paramref name="obj" /> to optional if predicate is equal true.
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="predicate">Predicate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Conditional.</returns>
        [NotNull]
        [ItemNotNull]
        public static async
#if NETSTANDARD2_0
            Task<Optional<T>>
#else
            ValueTask<Optional<T>>
#endif
            WhenAsync<T>([CanBeNull] this T obj, [InstantHandle] [NotNull] WhenAsyncDelegate predicate, CancellationToken cancellationToken = default)
        {
            ContractGuard.IfArgumentIsNull(nameof(predicate), predicate);

            return obj.When(await predicate(cancellationToken));
        }

        [NotNull]
        public delegate
#if NETSTANDARD2_0
            Task<bool>
#else
            ValueTask<bool>
#endif
            WhenAsyncDelegate<in T>([CanBeNull] T obj, CancellationToken cancellationToken = default);

        [NotNull]
        public delegate
#if NETSTANDARD2_0
            Task<bool>
#else
            ValueTask<bool>
#endif
            WhenAsyncDelegate(CancellationToken cancellationToken = default);

        public delegate bool WhenDelegate<in T>([CanBeNull] T obj);
    }
}
