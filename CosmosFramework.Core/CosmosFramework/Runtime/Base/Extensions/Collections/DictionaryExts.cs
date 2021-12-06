﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos
{
    public static class DictionaryExts
    {
        public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this IDictionary<TKey, TValue> @this)
        {
            return @this.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
        }
        public static  bool Remove<TKey, TValue>(this IDictionary<TKey, TValue> @this,TKey key)
        {
            return @this.Remove(key,out _);
        }
        public static bool TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key,out TValue value)
        {
            return @this.Remove(key, out value);
        }
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return @this.TryGetValue(key, out value) ? value : defaultValue;
        }
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> getDefaultValue)
        {
            TValue value;
            return @this.TryGetValue(key, out value) ? value : getDefaultValue();
        }

        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, TValue? defaultValue = null)
            where TValue : struct
        {
            TValue value;
            return @this.TryGetValue(key, out value) ? value : defaultValue;
        }
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key, Func<TValue> getDefaultValue)
        {
            TValue value;
            if (!@this.TryGetValue(key, out value))
                @this[key] = value = getDefaultValue();
            return value;
        }
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
            where TValue : new()
        {
            TValue value;
            if (!@this.TryGetValue(key, out value))
                @this[key] = value = new TValue();
            return value;
        }
        public static int GetAndIncrement<TKey>(this IDictionary<TKey, int> @this, TKey key, int startValue = 0)
        {
            int value;
            if (@this.TryGetValue(key, out value))
                @this[key] = ++value;
            else
                @this[key] = value = startValue;
            return value;
        }
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> @this, TKey key)
        {
            TValue value = default(TValue);
            bool isSuccess = @this.TryGetValue(key, out value);
            if (isSuccess)
                return value;
            return value;
        }
    }
}
