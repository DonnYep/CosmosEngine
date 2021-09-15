using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Network
{
    /// <summary>
    /// 网络通道Key；
    /// ChannelName 与 ChannelIPAddress 的组合；
    /// ChannelIPAddress 字段示例为 127.0.0.1:80
    /// </summary>
    public struct NetworkChannelKey: IEquatable<NetworkChannelKey>
    {
        public string ChannelName { get; private set; }
        /// <summary>
        /// 示例127.0.0.1:80
        /// </summary>
        public string ChannelIPAddress { get; private set; }
        public NetworkChannelKey(string tVar, string kVar)
        {
            ChannelName = tVar;
            ChannelIPAddress = kVar;
        }
        public bool Equals(NetworkChannelKey other)
        {
            return ChannelName==other.ChannelName && ChannelIPAddress==other.ChannelIPAddress;
        }
        public static bool operator ==(NetworkChannelKey a, NetworkChannelKey b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(NetworkChannelKey a, NetworkChannelKey b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return obj is NetworkChannelKey && Equals((NetworkChannelKey)obj);
        }
        public override int GetHashCode()
        {
            return ChannelName.GetHashCode() ^ ChannelIPAddress.GetHashCode();
        }
        public override string ToString()
        {
            if (string.IsNullOrEmpty( ChannelName))
                throw new ArgumentNullException($"ChannelName is  invalid");
            if (string.IsNullOrEmpty(ChannelIPAddress))
                throw new ArgumentNullException($"ChannelIPAddress is  invalid");
            return $"ChannelName :{ChannelName} ; ChannelIPAddress:{ChannelIPAddress}";
        }
    }
}
