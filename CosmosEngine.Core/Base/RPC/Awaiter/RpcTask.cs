using Cosmos.RPC.Core;
using System;
using System.Runtime.CompilerServices;
namespace Cosmos.RPC
{
    /// <summary>
    /// 无返回值RPC；
    /// </summary>
   public class RpcTask : INotifyCompletion, IRpcTask
    {
        public long TaskId { get; private set; }
        bool isCompleted;
        Action continuation;
        public bool IsCompleted
        {
            get
            { return isCompleted; }
            set
            {
                isCompleted = value;
                if (isCompleted)
                {
                    continuation?.Invoke();
                }
            }
        }
        public RpcTask(long rpcTaskId)
        {
            TaskId = rpcTaskId;
            RPCTaskManager.Instance.AddTask(this);
        }
        public void RspRpcData(byte[] returnDataBytes, Type returnDataType)
        {
            IsCompleted = true;
        }
        /// <summary>
        /// 由于无返回值，所以此方法无实现； 
        /// </summary>
        public void RspRpcSegment(int rspFullLength, byte[] segment) { }
        public RpcTask GetAwaiter() { return this; }
        public void GetResult() { }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }
}
