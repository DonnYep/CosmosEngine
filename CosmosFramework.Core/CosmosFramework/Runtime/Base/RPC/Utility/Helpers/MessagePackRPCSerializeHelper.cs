using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    public class MessagePackRPCSerializeHelper : RPCUtility.Serialization.IRPCSerializeHelper
    {
        public T Deserialize<T>(byte[] bytes)
        {
            return Utility.MessagePack.Deserialize<T>(bytes);
        }
        public object Deserialize(byte[] bytes, Type type)
        {
            return Utility.MessagePack.Deserialize(bytes, type);
        }
        public byte[] Serialize<T>(T obj)
        {
            return Utility.MessagePack.Serialize(obj);
        }
        public byte[] Serialize(object obj, Type type)
        {
            return Utility.MessagePack.Serialize(obj,type);
        }
        public byte[] SerializeBytes(object obj, Type type)
        {
            return Utility.MessagePack.Serialize(obj,type);
        }
        public byte[] SerializeToBytes(object obj)
        {
            return Utility.MessagePack.Serialize(obj);
        }
    }
}
