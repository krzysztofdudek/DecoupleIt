using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    [PublicAPI]
    internal static class DictionaryExtensions
    {
        [CanBeNull]
        public static TValue TryGetValue<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary, [NotNull] TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}
