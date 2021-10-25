using Cosmos.RPC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    public partial class RPCUtility
    {
        public class Serialization
        {
            static long RpcDataIndex = 0;
            public interface IRPCSerializeHelper
            {
                byte[] SerializeToBytes(object obj);
                byte[] Serialize<T>(T obj);
                T Deserialize<T>(byte[] bytes);
                object Deserialize(byte[] bytes, Type type);
            }
            static IRPCSerializeHelper rpcSerializeHelper;
            public static void SetHelper(IRPCSerializeHelper helper)
            {
                rpcSerializeHelper = helper;
            }
            public static byte[] SerializeBytes<T>(T obj)
            {
                return rpcSerializeHelper.Serialize(obj);
            }
            public static byte[] SerializeRpcDataToBytes(Type type, string methodName, Type retrunType, params object[] parameters)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                var length = parameters.Length;
                var argInfoArrary = new ParamData[length];
                for (int i = 0; i < length; i++)
                {
                    var param = parameters[i];
                    argInfoArrary[i] = new ParamData(param.GetType(), rpcSerializeHelper.SerializeToBytes(parameters[i]));
                }
                RPCData reqRpcData = new RPCData(RpcDataIndex++, type.FullName, methodName, new ParamData(retrunType, null), argInfoArrary);
                return rpcSerializeHelper.Serialize(reqRpcData);
            }
            public static RPCData SerializeToRpcData(Type type, string methodName, Type retrunType, params object[] parameters)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                var length = parameters.Length;
                var argInfoArrary = new ParamData[length];
                for (int i = 0; i < length; i++)
                {
                    var param = parameters[i];
                    argInfoArrary[i] = new ParamData(param.GetType(), rpcSerializeHelper.SerializeToBytes(parameters[i]));
                }
                return new RPCData(RpcDataIndex++, type.FullName, methodName, new ParamData(retrunType, null), argInfoArrary);
            }
            public static T Deserialize<T>(byte[] bytes)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                return rpcSerializeHelper.Deserialize<T>(bytes);
            }
            public static object Deserialize(byte[] bytes, Type type)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                return rpcSerializeHelper.Deserialize(bytes, type);
            }
        }
    }
}
