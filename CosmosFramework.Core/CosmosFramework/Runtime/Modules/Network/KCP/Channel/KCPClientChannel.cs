using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Network;
using kcp;

namespace Cosmos
{
    public class KCPClientChannel : INetworkChannel
    {
        KcpClientService kcpClientService;

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
        public NetworkProtocol NetworkProtocol { get; set; }
        public bool IsConnect { get; private set; }
        public NetworkChannelKey NetworkChannelKey { get; set; }
        public void Disconnect()
        {
            kcpClientService?.ServiceDisconnect();
        }
        public void TickRefresh()
        {
            kcpClientService?.ServiceTick();
        }
        public void Connect(string ip, ushort port)
        {
            kcpClientService = new KcpClientService();
            kcpClientService.ServiceSetup();
            kcpClientService.OnClientDataReceived += OnKCPReceiveDataHandler;
            kcpClientService.OnClientConnected += OnConnectHandler;
            kcpClientService.OnClientDisconnected += OnDisconnectHandler;
            kcpClientService.ServiceUnpause();
            kcpClientService.Port = port;
            kcpClientService.ServiceConnect(ip);
        }
        public void Disconnect(int connectionId)
        {
            kcpClientService?.ServiceDisconnect();
        }
        public void AbortChannel()
        {
            kcpClientService?.ServiceDisconnect();
        }
        public void SendMessage(NetworkReliableType reliableType, byte[] data, int connectionId)
        {
            var arraySegment = new ArraySegment<byte>(data);
            var byteType = (byte)reliableType;
            kcpClientService?.ServiceSend((KcpChannel)byteType, arraySegment);
        }
        void OnDisconnectHandler()
        {
            IsConnect = false;
            onDisconnected?.Invoke(NetworkChannelKey, -1);
            onConnected= null;
            onDisconnected = null;
            onReceiveData = null;
        }
        void OnConnectHandler()
        {
            IsConnect= true;
            Utility.Debug.LogWarning("Server Connected ! ");
            onConnected?.Invoke(NetworkChannelKey, -1);
        }
        void OnKCPReceiveDataHandler(ArraySegment<byte> arrSeg, byte channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, arrSeg.Offset, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(NetworkChannelKey, -1,rcvData);
        }
    }
}
