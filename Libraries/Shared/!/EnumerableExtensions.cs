using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Shared
{
    /// <summary>
    ///     Extends <see cref="IEnumerable{T}" />.
    /// </summary>
    [PublicAPI]
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Marks input as not null.
        /// </summary>
        /// <param name="object">Input object.</param>
        /// <typeparam name="T">Type.</typeparam>
        /// <returns>Not null value of type <typeparamref name="T" />.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [NotNull]
        [ItemNotNull]
        [LinqTunnel]
        public static IEnumerable<T> AsCollectionWithNotNullItems<T>([NotNull] this IEnumerable<T> @object)
        {
            return @object;
        }

        /// <summary>
        ///     Validates if list is not null and has not null items.
        /// </summary>
        /// <param name="enumerable">Enumerable.</param>
        /// <typeparam name="T">Item type.</typeparam>
        /// <returns>List.</returns>
        [NotNull]
        [ItemNotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [LinqTunnel]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MultipleNullableAttributesUsage")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConstantConditionalAccessQualifier")]
        public static List<T> ToNotNullList<T>(
            [NotNull] [ItemNotNull] [InstantHandle]
            this IEnumerable<T> enumerable)
        {
            var list = enumerable?.ToList();

            ContractGuard.IfArgumentNullOrContainsNullItems(nameof(enumerable), list);

            return list;
        }
    }
}
