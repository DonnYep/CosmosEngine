using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public static class ConcurrentQueueExts
    {
        public static void Clear<TValue>(this ConcurrentQueue<TValue> @this)
        {
            while (@this.Count > 0)
            {
                @this.TryDequeue(out _);
            }
        }
    }
}
