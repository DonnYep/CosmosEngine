using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public static class ConcurrentDictionaryExts
    {
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> @this, TKey key)
        {
            return @this.TryRemove(key, out _);
        }
    }
}
