using System;
using System.Collections.Generic;
using System.Text;
using kcp;
namespace Cosmos.RPC
{
    public class RPCServer
    {
        KcpServerService kcpServerService;
        Action<int> onConnected;
        Action<int> onDisconnected;
        Action<int, byte[]> onReceiveData;
        Action onAbort;

        public event Action OnAbort
        {
            add { onAbort += value; }
            remove { onAbort -= value; }
        }
        public event Action<int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public event Action<int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        public event Action<int, byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        public RPCServer()
        {
            kcpServerService = new KcpServerService();
        }
        public void Launch(ushort port)
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
        public void Abort()
        {
            kcpServerService?.ServicePause();
            kcpServerService.OnServerDataReceived -= OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected -= OnDisconnectedHandler;
            kcpServerService.OnServerConnected -= OnConnectedHandler;
            kcpServerService?.ServerServiceStop();
            onAbort?.Invoke();
        }
        /// <summary>
        /// 获取连接Id的地址；
        /// </summary>
        /// <param name="connectionId">连接Id</param>
        /// <returns></returns>
        public string GetConnectionAddress(int connectionId)
        {
            return kcpServerService.Server.GetClientAddress(connectionId);
        }
        public void TickRefresh()
        {
            kcpServerService?.ServiceTick();
        }
        void OnDisconnectedHandler(int conv)
        {
            onDisconnected?.Invoke(conv);
        }
        void OnConnectedHandler(int conv)
        {
            onConnected?.Invoke(conv);
        }
        void OnReceiveDataHandler(int conv, ArraySegment<byte> arrSeg, int Channel)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, 1, rcvData, 0, rcvLen);
            onReceiveData?.Invoke(conv, rcvData);
        }
    }
}
