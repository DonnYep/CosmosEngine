using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 并发泛型数据传输容器
    /// 封闭继承
    /// </summary>
    public class ConcurrentLogicEventArgs<T> : GameEventArgs
    {
        readonly object locker = new object();
        T data;
        public T Data
        {
            get
            {
                lock (locker)
                {
                    return data;
                }
            }
            private set
            {
                lock (locker)
                {
                    data = value;
                }
            }
        }
        /// <summary>
        /// 泛型构造
        /// </summary>
        /// <param name="data"></param>
        public ConcurrentLogicEventArgs(T data)
        {
            lock (locker)
            {
                SetData(data);
            }
        }
        public ConcurrentLogicEventArgs() { }
        /// <summary>
        /// 泛型数据类型
        /// </summary>
        public ConcurrentLogicEventArgs<T> SetData(T data)
        {
            lock (locker)
            {
                Data = data;
                return this;
            }
        }
        public override void Release()
        {
            lock (locker)
            {
                Data = default(T);
            }
        }
    }
}
