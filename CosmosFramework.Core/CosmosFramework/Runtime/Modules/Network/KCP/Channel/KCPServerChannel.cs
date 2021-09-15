using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Cosmos.Network;
using kcp;

namespace Cosmos
{
    public class KCPServerChannel : INetworkChannel
    {
        KcpServerService kcpServerService;


        Action<NetworkChannelKey, int> onConnected;
        Action<NetworkChannelKey, int> onDisconnected;
        Action<NetworkChannelKey, int, byte[]> onReceiveData;
        public event Action<NetworkChannelKey, int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public event Action<NetworkChannelKey, int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        public event Action<NetworkChannelKey, int, byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }

        public bool IsConnect { get { return kcpServerService.Server.IsActive(); } }
        public NetworkProtocol NetworkProtocol { get { return NetworkProtocol.KCP; } }
        public NetworkChannelKey NetworkChannelKey { get; set; }

        public void Connect(string ip, ushort port)
        {
            kcpServerService = new KcpServerService();
            kcpServerService.Port = port;
            kcpServerService.ServiceSetup();
            kcpServerService.ServiceUnpause();

            kcpServerService.OnServerDataReceived += OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected += OnDisconnectedHandler;
            kcpServerService.OnServerConnected += OnConnectedHandler;
            kcpServerService.ServiceConnect();
        }
        public void AbortChannel()
        {
            kcpServerService?.ServicePause();
            kcpServerService.OnServerDataReceived -= OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected -= OnDisconnectedHandler;
            kcpServerService.OnServerConnected -= OnConnectedHandler;
            kcpServerService?.ServerServiceStop();
        }
        public void TickRefresh()
        {
            kcpServerService?.ServiceTick();
        }
        public void Disconnect(int connectionId)
        {
            kcpServerService?.ServiceDisconnect(connectionId);
        }
        public void SendMessage(NetworkReliableType reliableType, byte[] data, int connectionId)
        {
            var segment = new ArraySegment<byte>(data);
            var byteType = (byte)reliableType;
            kcpServerService?.ServiceSend((KcpChannel)byteType, segment, connectionId);
        }
        void OnDisconnectedHandler(int conv)
        {
            onDisconnected?.Invoke(NetworkChannelKey,conv);
        }
        void OnConnectedHandler(int conv)
        {
            onConnected?.Invoke(NetworkChannelKey, conv);
        }
        void OnReceiveDataHandler(int conv, ArraySegment<byte> arrSeg, int Channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, 1, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(NetworkChannelKey, conv, rcvData);
        }
    }
}
