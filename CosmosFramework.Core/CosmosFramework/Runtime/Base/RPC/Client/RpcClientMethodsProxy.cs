using System;
using System.Collections.Generic;
using System.Text;
namespace Cosmos.RPC
{
    public class RpcClientMethodsProxy
    {
        public void Invoke(RPCData rspRpcData)
        {
            if (RPCTaskService.Instance.PeekTask(rspRpcData.RpcDataId, out var rpcTask))
            {
                rpcTask.RspRpcData(rspRpcData);
                RPCTaskService.Instance.RemoveTask(rspRpcData.RpcDataId);
            }
        }
    }
}
