using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Cosmos;

namespace Cosmos.Network
{
    /// <summary>
    /// UDP socket服务；
    /// 这里管理其他接入的远程对象；
    /// </summary>
    public class UdpService : INetworkService, IControllable
    {
        public bool IsPause { get; private set; }
        /// <summary>
        /// 对象IP
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// 对象端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// udpSocket对象
        /// </summary>
        protected UdpClient udpSocket;
        protected ConcurrentQueue<UdpReceiveResult> awaitHandle = new ConcurrentQueue<UdpReceiveResult>();
        protected long conv = 0;
        public event Action<long,ArraySegment<byte>> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        protected Action<long,ArraySegment<byte>> onReceiveData;

      protected  Action<long> onDisconnected;
        public event Action<long> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        protected Action<long> onConnected;
        public event Action<long> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public virtual void OnInitialization()
        {
            udpSocket = new UdpClient(Port);
            OnReceive();
        }
        /// <summary>
        /// 非空虚函数；
        /// 关闭这个服务；
        /// </summary>
        public virtual void OnTermination()
        {
            if (udpSocket != null)
            {
                udpSocket.Close();
                udpSocket = null;
            }
        }
        /// <summary>
        /// 异步接收网络消息接口
        /// </summary>
        public virtual async void OnReceive()
        {
            if (udpSocket != null)
            {
                try
                {
                    UdpReceiveResult result = await udpSocket.ReceiveAsync();
                    awaitHandle.Enqueue(result);
                    OnReceive();
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"Receive net message exception ：{e}");
                }
            }
        }
        /// <summary>
        /// 空虚函数;
        /// 发送报文信息
        /// </summary>
        /// <param name="netMsg">消息体</param>
        public virtual void SendMessageAsync(INetworkMessage netMsg) { }
        public virtual void SendMessageAsync(byte[] buffer, IPEndPoint endPoint) { }
        /// <summary>
        /// 空虚函数;
        /// 发送报文信息
        /// </summary>
        /// <param name="netMsg">消息体</param>
        /// <param name="endPoint">远程对象</param>
        public virtual void SendMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint) { }
        /// <summary>
        /// 非空虚函数；
        /// 轮询更新;
        /// </summary>
        public virtual void OnRefresh() { }
        public void OnPause() { IsPause = true; }
        public void OnUnPause() { IsPause = false; }

        public virtual void AbortUnavilablePeer(IRemotePeer peer) { }

    }
}
