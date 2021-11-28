using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public static class QueueExts
    {
        public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
        {
            foreach (T item in items)
                queue.Enqueue(item);
        }
    }
}
