using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using kcp;
namespace Cosmos.RPC
{
    public class RPCServer
    {
        RpcServerMethodsProxy methodsProxy;
        KcpServerService kcpServerService;
        Action<int> onConnected;
        Action<int> onDisconnected;
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
        public RPCServer(ushort port)
        {
            kcpServerService = new KcpServerService();
            kcpServerService.Port = port;
            methodsProxy = new RpcServerMethodsProxy(SendMessage);
        }
        public void Start()
        {
            kcpServerService.ServiceSetup();
            kcpServerService.ServiceUnpause();

            kcpServerService.OnServerDataReceived += OnReceiveDataHandler;
            kcpServerService.OnServerDisconnected += OnDisconnectedHandler;
            kcpServerService.OnServerConnected += OnConnectedHandler;
            kcpServerService.ServiceConnect();
            methodsProxy.RegisterAppDomainTypes();
        }
        public void SendMessage( int conv, byte[] data)
        {
            var segment = new ArraySegment<byte>(data);
            kcpServerService.ServiceSend(KcpChannel.Reliable, segment, conv);
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
        /// <param name="conv">连接Id</param>
        /// <returns></returns>
        public string GetConnectionAddress(int conv)
        {
            return kcpServerService.Server.GetClientAddress(conv);
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
            try
            {
                var rpcData = RPCUtility.Serialization.Deserialize<RPCData>(rcvData);
                methodsProxy.Invoke(conv, rpcData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
