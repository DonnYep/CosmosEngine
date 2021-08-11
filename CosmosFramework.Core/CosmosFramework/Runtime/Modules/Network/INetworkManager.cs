using kcp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cosmos
{
    public interface INetworkManager : IModuleManager
    {
        event Action<int> OnConnected;
        event Action<int> OnDisconnected;
        event Action<int,byte[]> OnReceiveData;

        void SendNetworkMessage(byte[] buffer, IPEndPoint endPoint);
        void SendNetworkMessage(KcpChannel channelId, byte[] data, int connectionId);
        void SendNetworkMessage(byte[] data, int connectionId);
        /// <summary>
        /// 与远程建立连接；
        /// 当前只有udp
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="protocolType">协议类型</param>
        void Connect( ushort port, NetworkProtocolType protocolType= NetworkProtocolType.KCP);
        /// <summary>
        /// 与指定的链接Id断开；
        /// </summary>
        /// <param name="connectionId">需要断开的会话Id</param>
        void Disconnect(int connectionId);
    }
}
