using System;
using System.Collections.Generic;
using System.Text;
using kcp;
namespace Cosmos.RPC
{
    public class RPCClient
    {
        Action onConnected;
        Action onDisconnected;
        Action<byte[]> onReceiveData;
        public event Action OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public event Action OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        public event Action<byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        KcpClientService kcpClientService;
        public bool IsConnect { get; private set; }

        public RPCClient()
        {
            kcpClientService = new KcpClientService();
        }
        public void Connect(string ip, ushort port)
        {
            kcpClientService.ServiceSetup();
            kcpClientService.OnClientDataReceived += OnReceiveDataHandler;
            kcpClientService.OnClientConnected += OnConnectHandler;
            kcpClientService.OnClientDisconnected += OnDisconnectHandler;
            kcpClientService.ServiceUnpause();
            kcpClientService.Port = port;
            kcpClientService.ServiceConnect(ip);
        }
        public void Disconnect()
        {
            kcpClientService.ServiceDisconnect();
        }
        public void TickRefresh()
        {
            kcpClientService?.ServiceTick();
        }
        void OnDisconnectHandler()
        {
            IsConnect = false;
            onDisconnected?.Invoke();
            onConnected = null;
            onDisconnected = null;
            onReceiveData = null;
        }
        void OnConnectHandler()
        {
            IsConnect = true;
            onConnected?.Invoke();
        }
        void OnReceiveDataHandler(ArraySegment<byte> arrSeg, byte channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, arrSeg.Offset, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(rcvData);
        }
    }
}
