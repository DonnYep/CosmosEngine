using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace Cosmos.RPC
{
    public class JsonRPCSerializeHelper : RPCUtility.Serialization.IRPCSerializeHelper
    {
        public T Deserialize<T>(byte[] bytes)
        {
            return Utility.Json.BytesToObject<T>(bytes);
        }
        public object Deserialize(byte[] bytes, Type type)
        {
            return Utility.Json.BytesToObject(bytes,type);
        }
        public byte[] Serialize<T>(T obj)
        {
            return Utility.Json.ToJsonBytes(obj);
        }
        public byte[] SerializeToBytes(object obj)
        {
            return Utility.Json.ToJsonBytes(obj);
        }
    }
}
