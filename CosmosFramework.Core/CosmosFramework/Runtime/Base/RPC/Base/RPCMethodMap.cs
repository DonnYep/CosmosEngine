using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmos.RPC.Core
{
    /// <summary>
    /// 实例方法调用者；
    /// 传入一个实例，远程调用这个对象的方法；
    /// </summary>
    internal class RPCMethodMap
    {
        Dictionary<RPCMethodKey, MethodInfo> methodDict;
        Action<int, RPCInvokeData> sendRspMessage;
        object instance;
        public RPCMethodMap(Action<int, RPCInvokeData> sendMessage)
        {
            sendRspMessage = sendMessage;
            methodDict = new Dictionary<RPCMethodKey, MethodInfo>();
        }
        public void SetInstance(object instance)
        {
            this.instance = instance;
        }
        public void AddMethod(MethodInfo methodInfo)
        {
            var paramArray = methodInfo.GetParameters();
            var key = new RPCMethodKey(methodInfo.Name, paramArray.Length);
            methodDict.TryAdd(key, methodInfo);
        }
        public void RemoveMethod(MethodInfo methodInfo)
        {
            var key = new RPCMethodKey(methodInfo.Name, methodInfo.GetParameters().Length);
            methodDict.Remove(key);
        }
        public bool InvokeMethod(int conv, RPCInvokeData rpcData)
        {
            bool result = false;
            var paramLength = rpcData.Parameters.Length;
            var methodKey = new RPCMethodKey(rpcData.MethodName, paramLength);
            if (methodDict.TryGetValue(methodKey, out var method))
            {
                result = true;
                var parameters = rpcData.Parameters;
                var length = paramLength;
                var paramDatas = new object[length];
                lock (methodDict)
                {
                    for (int i = 0; i < length; i++)
                    {
                        var obj = RPCUtility.Serialization.Deserialize(parameters[i].Value, parameters[i].ParameterType);
                        paramDatas[i] = obj;
                    }
                }
                if (typeof(Task).IsAssignableFrom(method.ReturnType))
                {
                    //Task只支持带参数泛型；
                    var retParamTypes = method.ReturnParameter.ParameterType.GetTypeInfo().GenericTypeArguments;
                    var resultData = AsyncInvokeMethod(method, paramDatas).Result;
                    if (retParamTypes.Length > 0)
                    {
                        var rspRpcData = rpcData.Clone();
                        var retType = retParamTypes[0];
                        var rstBin = RPCUtility.Serialization.Serialize(resultData, retType);
                        rspRpcData.ReturnData = new RPCParamData(retType, rstBin);
                        sendRspMessage.Invoke(conv, rspRpcData);
                    }
                    else
                    {
                        var rspRpcData = rpcData.Clone();
                        sendRspMessage.Invoke(conv, rspRpcData);
                    }
                }
                else
                {
                    var retParamTypes = method.ReturnParameter.ParameterType;
                    var resultData = method.Invoke(instance, paramDatas);
                    if (retParamTypes != typeof(void))
                    {
                        if (resultData != null)
                        {
                            var rspRpcData = rpcData.Clone();
                            var rstBin = RPCUtility.Serialization.Serialize(resultData, method.ReturnType);
                            rspRpcData.ReturnData = new RPCParamData(method.ReturnType, rstBin);
                            sendRspMessage.Invoke(conv, rspRpcData);
                        }
                        else
                        {
                            var rspRpcData = rpcData.Clone();
                            rspRpcData.ReturnData = new RPCParamData(method.ReturnType, new byte[0]);
                            sendRspMessage.Invoke(conv, rspRpcData);
                        }
                    }
                    else
                    {
                        var rspRpcData = rpcData.Clone();
                        rspRpcData.ReturnData = new RPCParamData(typeof(void), new byte[0]);
                        sendRspMessage.Invoke(conv, rspRpcData);
                    }
                }
            }
            return result;
        }
        public void Clear()
        {
            instance = null;
            methodDict.Clear();
        }
        async Task<object> AsyncInvokeMethod(MethodInfo method, object[] paramDatas)
        {
            var task = (Task)method.Invoke(instance, paramDatas);
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty.GetValue(task);
            return result;
        }
    }
}
