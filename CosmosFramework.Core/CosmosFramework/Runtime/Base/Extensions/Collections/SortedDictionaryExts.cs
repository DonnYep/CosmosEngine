using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public static class SortedDictionaryExts
    {
        public static void AddOrUpdate<TKey, TValue>(this SortedDictionary<TKey, TValue> @this, TKey key, TValue value)
        {
            if (@this.ContainsKey(key))
            {
                @this[key] = value;
            }
            else
            {
                @this.Add(key, value);
            }
        }
        public static bool Remove<TKey, TValue>(this SortedDictionary<TKey, TValue> @this, TKey key, out TValue value)
        {
            if (@this.ContainsKey(key))
            {
                value = @this[key];
                @this.Remove(key);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
        public static bool TryRemove<TKey, TValue>(this SortedDictionary<TKey, TValue> @this, TKey key, out TValue value)
        {
            if (@this.ContainsKey(key))
            {
                value = @this[key];
                @this.Remove(key);
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }
    }
}
