using System;

namespace Cosmos.RPC
{
    /// <summary>
    /// RPC服务。被标记的接口对象可以被自动创建代理对象；
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =false)]
    public class RPCServiceAttribute:Attribute{}
}
