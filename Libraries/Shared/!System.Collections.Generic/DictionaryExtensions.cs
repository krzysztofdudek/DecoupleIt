using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    ///     Class contains extensions for <see cref="Dictionary{TKey,TValue}" />.
    /// </summary>
    [PublicAPI]
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Tries to get value with specific key from dictionary. If key is not found <see langkey="null" /> will be returned.
        /// </summary>
        /// <param name="dictionary">Dictionary.</param>
        /// <param name="key">Key.</param>
        /// <typeparam name="TKey">Type of key.</typeparam>
        /// <typeparam name="TValue">Type of value.</typeparam>
        /// <returns>Value if key found, otherwise <see langword="null" />.</returns>
        [CanBeNull]
        public static TValue TryGetValue<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}
