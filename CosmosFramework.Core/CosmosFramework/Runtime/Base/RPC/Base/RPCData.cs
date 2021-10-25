using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    /// <summary>
    /// 调用时的数据；
    /// </summary>
    public struct RPCData
    {
        public long RpcDataId;
        public string TypeFullName;
        public string MethodName;
        public ParamData[] Parameters;
        public ParamData ReturnData;
        public RPCData(long rpcDataId, string typeFullName, string methodName, ParamData returnData, ParamData[] parameters)
        {
            RpcDataId = rpcDataId;
            TypeFullName = typeFullName;
            MethodName = methodName;
            Parameters = parameters;
            ReturnData = returnData;
        }
        public RPCData Clone()
        {
            return new RPCData(RpcDataId, TypeFullName, MethodName, ReturnData,Parameters);
        }
    }
}