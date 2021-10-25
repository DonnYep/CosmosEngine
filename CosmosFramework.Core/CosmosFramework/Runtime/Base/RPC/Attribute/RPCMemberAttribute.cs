using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.RPC
{
    /// <summary>
    /// RPC成员；
    /// </summary>
    [AttributeUsage(AttributeTargets.Method| AttributeTargets.Property| AttributeTargets.Field,AllowMultiple =false,Inherited =false)]
    public class RPCMemberAttribute:Attribute{}
}
