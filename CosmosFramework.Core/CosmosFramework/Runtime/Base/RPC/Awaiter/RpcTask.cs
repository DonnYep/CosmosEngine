using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos.RPC
{
    public class RpcTask<T> : INotifyCompletion,IRpcTask
    {
        public long TaskId { get; private set; }
        T rawData;
        Action continuation;
        bool isCompleted;
        RPCData reqRpcData;
        RPCData rspRpcData;

        int rspPackageFullLength;
        int curretPackageLength;
        byte[] rcvSeg= new byte[0];

        public RpcTask(T rawData)
        {
            this.rawData = rawData;
        }
        public RpcTask(RPCData reqRpcData)
        {
            this.reqRpcData = reqRpcData;
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
        public void RspRpcData(RPCData rspRpcData)
        {
            this.rspRpcData = rspRpcData;
            try
            {
                var returnData = rspRpcData.ReturnData;
                rawData = (T)RPCUtility.Serialization.Deserialize(rspRpcData.ReturnData.Value, rspRpcData.ReturnData.ParameterType);
            }
            catch (Exception e)
            {
                rawData = default ;
                Utility.Debug.LogError(e);
            }
            IsCompleted = true;
        }
        public void RspRpcSegment(int rspFullLength, byte[] segment)
        {
            rspPackageFullLength = rspFullLength;
            curretPackageLength += segment.Length;
            var localSegs = rcvSeg;
            rcvSeg = new byte[curretPackageLength];
            var localLength = localSegs.Length;
            Array.Copy(localSegs, 0, rcvSeg, 0, localLength);
            Array.Copy(segment, 0, rcvSeg, localLength, segment.Length);
            if (rspPackageFullLength == curretPackageLength)
            {
                var rpcData = RPCUtility.Serialization.Deserialize<RPCData>(rcvSeg);
                RspRpcData(rpcData);
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
