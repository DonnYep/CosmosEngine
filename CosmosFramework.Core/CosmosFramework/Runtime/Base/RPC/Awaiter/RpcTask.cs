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
        public T GetResult() { return rawData; }
        public RpcTask<T> GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            this.continuation = continuation;
        }
        //public static explicit operator T(RpcTask<T> rpcTask)
        //{
        //    return rpcTask.rawData;
        //}
        //public static implicit operator RpcTask<T>(T t)
        //{
        //    return new RpcTask<T>(t);
        //}
    }
}
