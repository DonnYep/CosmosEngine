using System;
using System.Runtime.CompilerServices;
namespace Cosmos.RPC
{
    public class RpcTask<T> : INotifyCompletion, IRpcTask
    {
        public long TaskId { get; private set; }
        T rawData;
        Action continuation;
        bool isCompleted;
        int curretPackageLength;
        byte[] rcvSeg = new byte[0];
        public RpcTask(T rawData)
        {
            this.rawData = rawData;
        }
        public RpcTask(RPCData reqRpcData)
        {
            TaskId = reqRpcData.RpcDataId;
            RPCTaskService.Instance.AddTask(this);
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
        public void RspRpcData(RPCData rpcData)
        {
            try
            {
                var returnData = rpcData.ReturnData;
                rawData = (T)RPCUtility.Serialization.Deserialize(rpcData.ReturnData.Value, rpcData.ReturnData.ParameterType);
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
        public T GetResult() { return rawData; }
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
