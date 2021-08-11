using System.Collections;
using Cosmos;
using System.Net;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using kcp;

namespace Cosmos.Network
{
    //TODO NetworkManager
    [Module]
    /// <summary>
    /// 此模块为客户端网络管理类
    /// </summary>
    internal sealed partial class NetworkManager : Module, INetworkManager
    {
        #region UDP
        INetworkService service;
        IHeartbeat heartbeat;
        #endregion

        #region KCP
        KcpServerService kcpServerService;
        #endregion

        public event Action<int, byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        Action<int, byte[]> onReceiveData;
        NetworkProtocolType currentNetworkProtocolType;
        public int PeerCount { get { return clientPeerDict.Count; } }
        ConcurrentDictionary<long, IRemotePeer> clientPeerDict = new ConcurrentDictionary<long, IRemotePeer>();
        Action<int> onConnected;
        public event Action<int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        Action<int> onDisconnected;
        public event Action<int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }

        public void SendNetworkMessage(byte[] buffer, IPEndPoint endPoint)
        {
            service.SendMessageAsync(buffer, endPoint);
        }
        public void SendNetworkMessage(KcpChannel channelId, byte[] data, int connectionId)
        {
            switch (currentNetworkProtocolType)
            {
                case NetworkProtocolType.TCP:
                    break;
                case NetworkProtocolType.UDP:
                    break;
                case NetworkProtocolType.KCP:
                    {
                        var segment = new ArraySegment<byte>(data);
                        kcpServerService.ServiceSend(channelId, segment, connectionId);
                    }
                    break;
            }
        }
        public void SendNetworkMessage(byte[] data, int connectionId)
        {
            SendNetworkMessage(KcpChannel.Reliable, data, connectionId);
        }
        /// <summary>
        /// 与远程建立连接；
        /// 当前只有udp
        /// </summary>
        /// <param name="ip">ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="protocolType">协议类型</param>
        public void Connect(ushort port, NetworkProtocolType protocolType = NetworkProtocolType.KCP)
        {
            OnUnPause();
            currentNetworkProtocolType = protocolType;
            switch (protocolType)
            {
                case NetworkProtocolType.KCP:
                    {
                        var kcpServer = new KcpServerService();
                        kcpServerService = kcpServer;
                        KCPLog.Info = (s) => Utility.Debug.LogInfo(s);
                        KCPLog.Warning = (s) => Utility.Debug.LogWarning(s);
                        KCPLog.Error = (s) => Utility.Debug.LogError(s);
                        kcpServerService.Port = (ushort)port;
                        kcpServerService.ServiceSetup();
                        kcpServerService.ServiceUnpause();
                        kcpServerService.OnServerDataReceived += OnKCPReceiveDataHandler;
                        kcpServerService.OnServerDisconnected += OnDisconnectedHandler;
                        kcpServerService.OnServerConnected += OnConnectedHandler;
                        kcpServerService.ServiceConnect();
                    }
                    break;
                case NetworkProtocolType.TCP:
                    {
                    }
                    break;
                case NetworkProtocolType.UDP:
                    {
                        //service = new UdpServerService();
                        //UdpServerService udp = service as UdpServerService;
                        //udp.OnReceiveData += OnReceiveDataHandler;
                        //udp.OnConnected+= OnConnectedHandler;
                        //udp.OnDisconnected+= OnDisconnectedHandler;
                        //udp.Port = port;
                        //service.OnInitialization();
                    }
                    break;
            }
        }
        /// <summary>
        /// 与指定的会话Id断开；
        /// </summary>
        /// <param name="connectionId">需要断开的会话Id</param>
        public void Disconnect(int connectionId)
        {
            switch (currentNetworkProtocolType)
            {
                case NetworkProtocolType.TCP:
                    break;
                case NetworkProtocolType.UDP:
                    break;
                case NetworkProtocolType.KCP:
                    kcpServerService?.ServiceDisconnect(connectionId);
                    break;
            }
        }
        protected override void OnInitialization()
        {
            IsPause = false;
        }
        [TickRefresh]
        void OnRefresh()
        {
            if (IsPause)
                return;
            switch (currentNetworkProtocolType)
            {
                case NetworkProtocolType.TCP:
                    break;
                case NetworkProtocolType.UDP:
                    // service?.OnRefresh();
                    break;
                case NetworkProtocolType.KCP:
                    kcpServerService?.ServiceTick();
                    break;
            }
        }
        void OnKCPReceiveDataHandler(int conv, ArraySegment<byte> arrSeg, int Channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, 1, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(conv, rcvData);
        }
        void OnDisconnectedHandler(int conv)
        {
            onDisconnected?.Invoke(conv);
        }
        void OnConnectedHandler(int conv)
        {
            onConnected?.Invoke(conv);
        }
    }
}
