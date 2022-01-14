using System;
namespace Cosmos.RPC
{
    /// <summary>
    /// 此特性会标记一个类对象，并将类的所有方法函数进行RPC标记；
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple =false,Inherited =false)]
    public class RPCAttribute : Attribute 
    {
        public bool IncludeOriginalMethods { get; private set; }
        /// <summary>
        ///默认构造，是否包含原始方法； 
        /// </summary>
        public RPCAttribute(bool includeOriginalMethods=false)
        {
            IncludeOriginalMethods = includeOriginalMethods;
        }
        /// <summary>
        /// object 类型原始自带的方法名；
        /// </summary>
        public readonly static string[] OriginalMethods = new string[] { "GetType", "GetHashCode", "ToString", "Equals" };
    }
}
