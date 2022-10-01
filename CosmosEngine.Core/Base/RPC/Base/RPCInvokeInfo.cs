namespace Cosmos.RPC
{
    public struct RPCInvokeInfo
    {
        public long RpcDataId;
        public byte[] RpcDataBytes;
        public RPCInvokeInfo(long rpcDataId, byte[] rpcDataBytes)
        {
            RpcDataId = rpcDataId;
            RpcDataBytes = rpcDataBytes;
        }
    }
}
