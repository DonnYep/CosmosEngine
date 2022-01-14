using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using Cosmos.RPC.Core;

namespace Cosmos.RPC.Server
{
    internal class RpcServerMethodsProxy
    {
        ConcurrentDictionary<Type, RPCMethodMap> methodDict;
        Dictionary<string, Type> stringTypeDict;
        Action<int, RPCInvokeData> sendRspMessage;
        public RpcServerMethodsProxy(Action<int, RPCInvokeData> sendMessage)
        {
            sendRspMessage = sendMessage;
            methodDict = new ConcurrentDictionary<Type, RPCMethodMap>();
            stringTypeDict = new Dictionary<string, Type>();
        }
        public void RegisterAppDomainTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
            }
        }
        public void RegisterAssemblyTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                Register(type);
            }
        }
        public void DeregisterAssemblyTypes(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                Deregister(type);
            }
        }
        public void SetMethodInvoker(Type type, object instance)
        {
            if (methodDict.TryGetValue(type, out var miMap))
            {
                miMap.SetInstance(instance);
            }
        }
        public void Register(Type type)
        {
            if (!methodDict.ContainsKey(type))
            {
                var miMap = new RPCMethodMap(sendRspMessage);
                methodDict.TryAdd(type, miMap);
                stringTypeDict.TryAdd(type.FullName, type);
                var methods = Utility.Assembly.GetTypeMethodsByAttribute<RPCMemberAttribute>(type);
                var length = methods.Length;
                for (int i = 0; i < length; i++)
                {
                    var method = methods[i];
                    miMap.AddMethod(method);
                }
            }
        }
        public void Deregister(Type type)
        {
            if (!methodDict.TryRemove(type, out var miMap))
            {
                miMap.Clear();
                stringTypeDict.Remove(type.FullName);
            }
        }
        public bool InvokeReq(int conv,RPCInvokeData reqRpcData)
        {
            var result = false;
            if (stringTypeDict.TryGetValue(reqRpcData.TypeFullName, out var type))
            {
                result = true;
                methodDict[type].InvokeMethod(conv,reqRpcData);
            }
            return result;
        }
        public void Clear()
        {
            methodDict.Clear();
            stringTypeDict.Clear();
            sendRspMessage = null;
        }
    }
}
