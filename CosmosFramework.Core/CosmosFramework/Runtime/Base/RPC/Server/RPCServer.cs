using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using kcp;
using Cosmos.RPC.Core;

namespace Cosmos.RPC.Server
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
        void SendRpcData(int conv, RPCInvokeData rpcData)
        {
            if (rpcData.ReturnData.Value.Length <= RPCConstants.MaxRpcPackSize)
            {
                var srcData = RPCUtility.Serialization.Serialize(rpcData);
                var sndData = new byte[srcData.Length + 1];
                sndData[0] = (byte)RPCPackageType.Fullpackage;
                Array.Copy(srcData, 0, sndData, 1, srcData.Length);
                SendMessage(conv, sndData);
            }
            else
            {
                rpcSubpackageProcesser.AddFullpackage(conv, rpcData);
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
                var rpcData = RPCUtility.Serialization.Deserialize<RPCInvokeData>(rcvData);
                methodsProxy.InvokeReq(conv, rpcData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
