using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cosmos
{
    /// <summary>
    /// 可调度的线程锁对象
    /// 提供同步访问对象的机制。
    /// Lock锁与Monitor区别：
    /// https://www.cnblogs.com/chenwolong/p/7503977.html
    /// </summary>
    public class DispatchableLocker
    {
        private object lockedObject;
        public bool IsLocked { get; private set; }
        Action lockFailHandler;
        Action lockSuccessHandler;
        public DispatchableLocker(object obj)
        {
            if (!Monitor.TryEnter(obj))
            {
                return;
            }
            this.IsLocked = true;
            this.lockedObject = obj;
        }
        public DispatchableLocker(object obj, Action lockFailHandler)
        {
            this.lockFailHandler += lockFailHandler;
        }
        public DispatchableLocker(object obj, Action lockSuccessHandler, Action lockFailHandler)
        {
            this.lockFailHandler += lockFailHandler;
            this.lockSuccessHandler += lockSuccessHandler;
        }
        /// <summary>
        /// 调用事件
        /// </summary>
        public void Dispatch()
        {
            try
            {
                Monitor.Enter(lockedObject);
                lockSuccessHandler?.Invoke();
            }
            catch (Exception)
            {
                lockFailHandler?.Invoke();
            }
            finally
            {
                Monitor.Exit(lockedObject);
            }
        }
        public void Dispose()
        {
            if (!this.IsLocked)
            {
                return;
            }
            Monitor.Exit(this.lockedObject);
            this.lockedObject = null;
            this.IsLocked = false;
            lockSuccessHandler = null;
            lockFailHandler = null;
        }
    }
}
