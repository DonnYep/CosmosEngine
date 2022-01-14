using System;
using Cosmos.RPC.Core;
namespace Cosmos.RPC.Server
{
    public class RPCServer
    {
        Telepathy.Server server;
        RpcServerMethodsProxy methodsProxy;

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
        public RPCServer()
        {
            server = new Telepathy.Server(RPCConstants.TcpMaxMessageSize);
            methodsProxy = new RpcServerMethodsProxy(SendRpcData);
            rpcSubpackageProcesser = new RpcSubpackageProcesser(SendMessage);
        }
        public void Start(int port)
        {
            server.OnConnected += OnConnectedHandler;
            server.OnDisconnected += OnDisconnectedHandler;
            server.OnData += OnDataHandler;
            server.Start(port);
            methodsProxy.RegisterAppDomainTypes();
        }
        public void Stop()
        {
            server.OnConnected -= OnConnectedHandler;
            server.OnDisconnected -= OnDisconnectedHandler;
            server.OnData -= OnDataHandler;
            onAbort?.Invoke();
        }
        /// <summary>
        /// 获取连接Id的地址；
        /// </summary>
        /// <param name="conv">连接Id</param>
        /// <returns></returns>
        public string GetConnectionAddress(int conv)
        {
            return server.GetClientAddress(conv);
        }
        public void TickRefresh()
        {
            server.Tick(100);
            rpcSubpackageProcesser.TickRefresh();
        }
        /// <summary>
        /// 发送byte stream
        /// </summary>
        void SendMessage(int conv, byte[] data)
        {
            var segment = new ArraySegment<byte>(data);
            server.Send(conv, segment);
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
        void OnDataHandler(int conv, ArraySegment<byte> arrSeg)
        {
            try
            {
                var rpcData = RPCUtility.Serialization.Deserialize<RPCInvokeData>(arrSeg.Array);
                methodsProxy.InvokeReq(conv, rpcData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
