using System.Collections.Generic;
using JetBrains.Annotations;

namespace GS.DecoupleIt.Optionals
{
    /// <summary>
    ///     Extends <see cref="IDictionary{TKey,TValue}" /> with methods using optionals.
    /// </summary>
    [PublicAPI]
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Tries to get value with given key.
        /// </summary>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="key">Key.</param>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <returns>Optional.</returns>
        public static Optional<TValue> TryGetValue<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
        {
            dictionary.TryGetValue(key, out var value);

            return new Optional<TValue>(value);
        }
    }
}
