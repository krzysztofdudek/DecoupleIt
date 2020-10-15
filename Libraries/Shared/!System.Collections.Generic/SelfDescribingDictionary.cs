using System.Text;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    internal sealed class SelfDescribingDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        [NotNull]
        public override string ToString()
        {
            if (Count == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var isFirst       = true;

            foreach (var item in this)
            {
                if (!isFirst)
                    stringBuilder.Append(", ");

                stringBuilder.Append($"{item.Key}: {item.Value}");

                isFirst = false;
            }

            return stringBuilder.ToString();
        }
    }
}
