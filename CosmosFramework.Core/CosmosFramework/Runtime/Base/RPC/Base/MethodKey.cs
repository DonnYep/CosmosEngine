using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Cosmos.RPC
{
    internal struct MethodKey : IEquatable<MethodKey>
    {
        readonly string methodFullName ;
        readonly int methodParameterCount;
        public string MethodFullName { get { return methodFullName; } }

        public int MethodParameterCount { get {return methodParameterCount; } }
        readonly int hashCode;
        public MethodKey(string methodFullName, int paramCount)
        {
            if (string.IsNullOrEmpty(methodFullName))
                throw new ArgumentException($"{nameof(methodFullName)} is invalid !");
            this.methodFullName= methodFullName;
            this.methodParameterCount = paramCount;
            hashCode = methodFullName.GetHashCode() ^ paramCount.GetHashCode();
        }
        public bool Equals(MethodKey other)
        {
            return methodFullName== other.methodFullName&& methodParameterCount== other.methodParameterCount;
        }
        public static bool operator ==(MethodKey a, MethodKey b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(MethodKey a, MethodKey b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is MethodKey  && Equals((MethodKey)obj);
        }
        public override int GetHashCode()
        {
            return hashCode;
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty(methodFullName))
                throw new ArgumentNullException($"{nameof(methodFullName)}is  invalid");
            return $"MethodFullName: {methodFullName} ; MethodParameterCount: {methodParameterCount}";
        }
    }
}
