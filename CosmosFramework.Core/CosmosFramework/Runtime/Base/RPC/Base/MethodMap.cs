using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace Cosmos.RPC
{
    /// <summary>
    /// 实例方法调用者；
    /// 传入一个实例，远程调用这个对象的方法；
    /// </summary>
    public class MethodMap
    {
        Dictionary<MethodKey, MethodInfo> methodDict;
        Action<int, RPCData> sendRspMessage;
        object instance;
        public MethodMap(Action<int, RPCData> sendMessage)
        {
            sendRspMessage = sendMessage;
            methodDict = new Dictionary<MethodKey, MethodInfo>();
        }
        public void SetInstance(object instance)
        {
            this.instance = instance;
        }
        public void AddMethod(MethodInfo methodInfo)
        {
            var paramArray = methodInfo.GetParameters();
            var key = new MethodKey(methodInfo.Name, paramArray.Length);
            methodDict.TryAdd(key, methodInfo);
        }
        public void RemoveMethod(MethodInfo methodInfo)
        {
            var key = new MethodKey(methodInfo.Name, methodInfo.GetParameters().Length);
            methodDict.Remove(key);
        }
        public bool InvokeMethod(int conv, RPCData rpcData)
        {
            bool result = false;
            var paramLength = rpcData.Parameters.Length;
            var methodKey = new MethodKey(rpcData.MethodName, paramLength);
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
                    var paramTypes = method.ReturnParameter.ParameterType.GetTypeInfo().GenericTypeArguments;
                    var resultData = AsyncInvokeMethod(method, paramDatas).Result;
                    if (resultData != null)
                    {
                        var rspRpcData = rpcData.Clone();
                        var retType = paramTypes[0];
                        var rstBin = RPCUtility.Serialization.Serialize(resultData, retType);
                        rspRpcData.ReturnData = new ParamData(retType, rstBin);
                        sendRspMessage.Invoke(conv, rspRpcData);
                    }
                }
                else
                {
                    var resultData = method.Invoke(instance, paramDatas);
                    if (resultData != null)
                    {
                        var rspRpcData = rpcData.Clone();
                        var rstBin = RPCUtility.Serialization.Serialize(resultData, method.ReturnType);
                        rspRpcData.ReturnData = new ParamData(method.ReturnType, rstBin);
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
            Utility.Debug.LogInfo("AsyncInvokeMethod<->" + method.Name);
            var task = (Task)method.Invoke(instance, paramDatas);
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty.GetValue(task);
            return result;
        }
    }
}
