using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    /// <summary>
    ///     Extends <see cref="object" />.
    /// </summary>
    [PublicAPI]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Marks input as not null.
        /// </summary>
        /// <param name="object">Input object.</param>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Not null value of type <typeparamref name="T" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [NotNull]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public static T AsNotNull<T>([CanBeNull] this T @object)
        {
            return @object;
        }

        /// <summary>
        ///     Executes action if object is not null.
        /// </summary>
        /// <param name="object">Object.</param>
        /// <param name="actionIfNotNull">Action.</param>
        /// <typeparam name="T">Type of an object.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IfNotNull<T>([CanBeNull] this T @object, [NotNull] Action<T> actionIfNotNull)
        {
            if (@object == null)
                return;

            actionIfNotNull(@object);
        }

        /// <summary>
        ///     Executes action if object is not null.
        /// </summary>
        /// <param name="object">Object.</param>
        /// <param name="actionIfNotNull">Action.</param>
        /// <typeparam name="T">Type of an object.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [CanBeNull]
        public static TResult IfNotNull<T, TResult>([CanBeNull] this T @object, [NotNull] Func<T, TResult> actionIfNotNull)
        {
            return @object == null ? default : actionIfNotNull(@object);
        }
    }
}
