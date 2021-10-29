using System;
using System.Collections.Generic;
using System.Text;
namespace Cosmos.RPC
{
    /// <summary>
    /// 参数数据；
    /// </summary>
    internal struct ParamData
    {
        public Type ParameterType;
        public byte[] Value;

        public ParamData(Type parameterType, byte[] value)
        {
            ParameterType = parameterType;
            Value = value;
        }
    }
}
