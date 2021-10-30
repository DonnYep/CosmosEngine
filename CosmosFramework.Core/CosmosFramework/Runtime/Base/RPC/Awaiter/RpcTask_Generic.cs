using Cosmos.RPC.Core;
using System;
using System.Runtime.CompilerServices;
namespace Cosmos.RPC
{
    /// <summary>
    /// 带参返回RPC；
    /// </summary>
    public class RpcTask<T> : INotifyCompletion, IRpcTask
    {
        public long TaskId { get; private set; }
        T rawData;

        bool isCompleted;
        Action continuation;
        int curretPackageLength;
        byte[] rcvSeg = new byte[0];
        public RpcTask(long rpcTaskId)
        {
            TaskId = rpcTaskId;
            RPCTaskManager.Instance.AddTask(this);
        }
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
        public void RspRpcData(byte[] returnDataBytes, Type returnDataType)
        {
            try
            {
                rawData = (T)RPCUtility.Serialization.Deserialize(returnDataBytes, returnDataType);
            }
            catch (Exception e)
            {
                rawData = default;
                Utility.Debug.LogError(e);
            }
            IsCompleted = true;
        }
        public void RspRpcSegment(int rspFullLength, byte[] segment)
        {
            if (rcvSeg.Length != rspFullLength)
                rcvSeg = new byte[rspFullLength];
            Array.Copy(segment, 0, rcvSeg, curretPackageLength, segment.Length);
            curretPackageLength += segment.Length;
            if (rspFullLength == curretPackageLength)
            {
                try
                {
                    rawData = RPCUtility.Serialization.Deserialize<T>(rcvSeg);
                }
                catch (Exception e)
                {
                    rawData = default;
                    Utility.Debug.LogError(e);
                }
                IsCompleted = true;
            }
        }
        public T GetResult(){return rawData;}
        public RpcTask<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
    }
}
