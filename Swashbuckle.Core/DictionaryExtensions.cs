using System.Collections.Generic;

namespace Swashbuckle.Core
{
    public static class DictionaryExtensions
    {
        public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> other)
        {
            foreach (var entry in other)
            {
                dictionary[entry.Key] = entry.Value;
            }
        }
    }
}