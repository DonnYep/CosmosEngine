using System;
using System.Collections.Generic;
using System.Text;
namespace Cosmos.RPC
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =false)]
    public class RPCAttribute:Attribute{}
}
