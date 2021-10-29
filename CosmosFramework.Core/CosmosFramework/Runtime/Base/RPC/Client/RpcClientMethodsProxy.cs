using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Cosmos.RPC
{
    internal class RpcClientMethodsProxy
    {
        public void InvokeRsp(byte[] fullpackage)
        {
            var rspRpcData = RPCUtility.Serialization.Deserialize<RPCData>(fullpackage);
            if (RPCTaskService.Instance.PeekTask(rspRpcData.RpcDataId, out var rpcTask))
            {
                rpcTask.RspRpcData(rspRpcData);
            }
        }
        public void InvokeRspSegment(byte[] rpcDataSeg)
        {
            var seg = RPCDataSegment.Deserialize(rpcDataSeg);
            if (RPCTaskService.Instance.PeekTask(seg.RspDataId, out var rpcTask))
            {
                rpcTask.RspRpcSegment(seg.RspDataLength, seg.Segment);
            }
        }
    }
}
