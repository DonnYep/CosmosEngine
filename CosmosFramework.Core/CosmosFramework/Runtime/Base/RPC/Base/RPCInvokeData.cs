namespace Cosmos.RPC.Core
{
    /// <summary>
    /// 调用时的数据；
    /// </summary>
    public struct RPCInvokeData
    {
        public long RpcDataId;
        public string TypeFullName;
        public string MethodName;
        public RPCParamData[] Parameters;
        public RPCParamData ReturnData;
        public RPCInvokeData(long rpcDataId, string typeFullName, string methodName, RPCParamData returnData, RPCParamData[] parameters)
        {
            RpcDataId = rpcDataId;
            TypeFullName = typeFullName;
            MethodName = methodName;
            Parameters = parameters;
            ReturnData = returnData;
        }
        public RPCInvokeData Clone()
        {
            return new RPCInvokeData(RpcDataId, TypeFullName, MethodName, ReturnData, Parameters);
        }
    }
}