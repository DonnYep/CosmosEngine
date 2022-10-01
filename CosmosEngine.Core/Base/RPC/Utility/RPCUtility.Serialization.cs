using Cosmos.RPC.Core;
using System;
namespace Cosmos.RPC
{
    public partial class RPCUtility
    {
        public class Serialization
        {
            static long RpcDataIndex = 0;
            public interface IRPCSerializeHelper
            {
                byte[] Serialize(object obj, Type type);
                byte[] Serialize<T>(T obj);
                T Deserialize<T>(byte[] bytes);
                object Deserialize(byte[] bytes, Type type);
            }
            static IRPCSerializeHelper rpcSerializeHelper;
            public static void SetHelper(IRPCSerializeHelper helper)
            {
                rpcSerializeHelper = helper;
            }
            public static byte[] Serialize<T>(T obj)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                return rpcSerializeHelper.Serialize(obj);
            }
            public static byte[] Serialize(object obj, Type type)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                return rpcSerializeHelper.Serialize(obj, type);
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
            public static RPCInvokeInfo EncodeRpcData(string typeFullName, string methodName, Type retrunType, params object[] parameters)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                var length = parameters != null ? parameters.Length : 0;
                var argInfoArrary = new RPCParamData[length];
                for (int i = 0; i < length; i++)
                {
                    var param = parameters[i];
                    var pType = param.GetType();
                    argInfoArrary[i] = new RPCParamData(pType, rpcSerializeHelper.Serialize(parameters[i], pType));
                }
                var rpcData = new RPCInvokeData(RpcDataIndex++, typeFullName, methodName, new RPCParamData(retrunType, null), argInfoArrary);
                var rpcDatabytes = rpcSerializeHelper.Serialize(rpcData);
                return new RPCInvokeInfo(rpcData.RpcDataId, rpcDatabytes);
            }
        }
    }
}
