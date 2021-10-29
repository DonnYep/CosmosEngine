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
            internal static RPCData EncodeRpcData(string typeFullName, string methodName, Type retrunType, params object[] parameters)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                if (parameters == null)
                    return new RPCData(RpcDataIndex++, typeFullName, methodName, new ParamData(retrunType, null), new ParamData[0]);
                var length = parameters.Length;
                var argInfoArrary = new ParamData[length];
                for (int i = 0; i < length; i++)
                {
                    var param = parameters[i];
                    var pType = param.GetType();
                    argInfoArrary[i] = new ParamData(pType, rpcSerializeHelper.Serialize(parameters[i], pType));
                }
                return new RPCData(RpcDataIndex++, typeFullName, methodName, new ParamData(retrunType, null), argInfoArrary);
            }
            internal static byte[] EncodeRpcDataToBytes(string typeFullName, string methodName, Type retrunType, params object[] parameters)
            {
                if (rpcSerializeHelper == null)
                    throw new ArgumentNullException("IRPCSerializeHelper is invalid !");
                RPCData reqRpcData;
                if (parameters == null)
                {
                    reqRpcData = new RPCData(RpcDataIndex++, typeFullName, methodName, new ParamData(retrunType, null), new ParamData[0]);
                    return rpcSerializeHelper.Serialize(reqRpcData);
                }
                var length = parameters.Length;
                var argInfoArrary = new ParamData[length];
                for (int i = 0; i < length; i++)
                {
                    var param = parameters[i];
                    var pType = param.GetType();
                    argInfoArrary[i] = new ParamData(pType, rpcSerializeHelper.Serialize(parameters[i], pType));
                }
                reqRpcData = new RPCData(RpcDataIndex++, typeFullName, methodName, new ParamData(retrunType, null), argInfoArrary);
                return rpcSerializeHelper.Serialize(reqRpcData);
            }
        }
    }
}
