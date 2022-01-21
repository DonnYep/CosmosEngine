﻿using System;
namespace Cosmos
{
    /// <summary>
    ///跳表的数据节点；
    /// </summary>
    public class SkipListNode<T> : IDisposable
          where T : IComparable
    {
        private T value;
        private SkipListNode<T> next;
        private SkipListNode<T> previous;
        private SkipListNode<T> above;
        private SkipListNode<T> below;
        public virtual T Value { get { return value; } set { this.value = value; } }
        public SkipListNode<T> Next { get { return next; } set { next = value; } }
        public SkipListNode<T> Previous { get { return previous; } set { previous = value; } }
        public SkipListNode<T> Above { get { return above; } set { above = value; } }
        public SkipListNode<T> Below { get { return below; } set { below = value; } }
        public SkipListNode(T value)
        {
            this.value = value;
        }
        public void Dispose()
        {
            value = default(T);
            next = null;
            previous = null;
            above = null;
            previous = null;
        }
    }
}
