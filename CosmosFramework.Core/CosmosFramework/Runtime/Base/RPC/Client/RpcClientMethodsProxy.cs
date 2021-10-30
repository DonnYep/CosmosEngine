using Cosmos.RPC.Core;
namespace Cosmos.RPC.Client
{
    internal class RpcClientMethodsProxy
    {
        public void InvokeRsp(byte[] fullpackage)
        {
            var rspRpcData = RPCUtility.Serialization.Deserialize<RPCInvokeData>(fullpackage);
            if (RPCTaskManager.Instance.PeekTask(rspRpcData.RpcDataId, out var rpcTask))
            {
                rpcTask.RspRpcData(rspRpcData.ReturnData.Value,rspRpcData.ReturnData.ParameterType);
            }
        }
        public void InvokeRspSegment(byte[] rpcDataSeg)
        {
            var seg = RPCDataSegment.Deserialize(rpcDataSeg);
            if (RPCTaskManager.Instance.PeekTask(seg.RspDataId, out var rpcTask))
            {
                rpcTask.RspRpcSegment(seg.RspDataLength, seg.Segment);
            }
        }
    }
}
