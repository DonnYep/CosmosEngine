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
        RpcSubpackageProcesser rpcSubpackageProcesser;
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
            methodsProxy = new RpcServerMethodsProxy(SendRpcData);
            rpcSubpackageProcesser = new RpcSubpackageProcesser(SendMessage);
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
            rpcSubpackageProcesser.TickRefresh();
        }
        /// <summary>
        /// 发送byte stream
        /// </summary>
        void SendMessage(int conv, byte[] data)
        {
            var segment = new ArraySegment<byte>(data);
            kcpServerService.ServiceSend(KcpChannel.Reliable, segment, conv);
        }
        /// <summary>
        /// 发送rpcdata;
        /// </summary>
        void SendRpcData(int conv, RPCData rpcData)
        {
            var data = RPCUtility.Serialization.SerializeBytes(rpcData);
            if (data.Length <= RPCConstants.MaxRpcPackSize)
            {
                var fullpackageData = new byte[data.Length + 1];
                fullpackageData[0] = (byte)RPCDataPackageType.Fullpackage;
                Array.Copy(data, 0, fullpackageData, 1, data.Length);
                SendMessage(conv, fullpackageData);
            }
            else
            {
                rpcSubpackageProcesser.AddFullpackage(conv, rpcData.RpcDataId, data);
            }
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
                methodsProxy.InvokeReq(conv, rpcData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
