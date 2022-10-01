using System;
namespace Cosmos.RPC.Core
{
    /// <summary>
    /// 参数数据；
    /// </summary>
    public struct RPCParamData
    {
        public Type ParameterType;
        public byte[] Value;
        public RPCParamData(Type parameterType, byte[] value)
        {
            ParameterType = parameterType;
            Value = value;
        }
    }
}
