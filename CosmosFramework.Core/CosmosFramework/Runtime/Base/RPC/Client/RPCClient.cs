using kcp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
namespace Cosmos.RPC
{
    public class RPCClient
    {
        Action onConnected;
        Action onDisconnected;
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
        KcpClientService kcpClientService;
        RpcClientMethodsProxy methodsProxy;
        public bool IsConnect { get; private set; }
        public RPCClient()
        {
            kcpClientService = new KcpClientService();
            methodsProxy = new RpcClientMethodsProxy();
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
        public void SendMessage(byte[] data)
        {
            var arraySegment = new ArraySegment<byte>(data);
            kcpClientService.ServiceSend(KcpChannel.Reliable, arraySegment);
        }
        public void Disconnect()
        {
            kcpClientService?.ServiceDisconnect();
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
            try
            {
                var rpcData = RPCUtility.Serialization.Deserialize<RPCData>(rcvData);
                methodsProxy.Invoke(rpcData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
