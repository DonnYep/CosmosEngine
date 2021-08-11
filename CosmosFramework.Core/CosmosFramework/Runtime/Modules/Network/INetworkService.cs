using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Cosmos
{
    /// <summary>
    /// 网络服务接口
    /// </summary>
    public interface INetworkService : IBehaviour, IRefreshable, IControllable
    {
        event Action<long,ArraySegment<byte>> OnReceiveData;
        event Action<long> OnDisconnected;
        event Action<long> OnConnected;
        /// <summary>
        /// 发送网络消息;
        /// </summary>
        /// <param name="netMsg">网络消息数据对象</param>
        /// <param name="endPoint">远程对象</param>
        void SendMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint);
        void SendMessageAsync(byte[] buffer, IPEndPoint endPoint);
        /// <summary>
        /// 发送网络消息;
        /// </summary>
        /// <param name="netMsg">网络消息数据对象</param>
        void SendMessageAsync(INetworkMessage netMsg);
        /// <summary>
        /// 接收网络消息
        /// </summary>
        void OnReceive();
        /// <summary>
        /// 移除失效peer；
        /// 作为参数传入peer；
        /// </summary>
        /// <param name="peer">失效的peerD</param>
        void AbortUnavilablePeer(IRemotePeer peer);
    }
}
