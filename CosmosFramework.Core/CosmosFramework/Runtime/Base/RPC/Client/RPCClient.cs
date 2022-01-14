using System;
using Cosmos.RPC.Core;

namespace Cosmos.RPC.Client
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
        
        Telepathy.Client client;
        RpcClientMethodsProxy methodsProxy;
        public bool IsConnect { get; private set; }
        public RPCClient()
        {
            client = new Telepathy.Client(RPCConstants.TcpMaxMessageSize);
            methodsProxy = new RpcClientMethodsProxy();
        }
        public void Connect(string ip, int port)
        {
            client.OnConnected += OnConnectHandler;
            client.OnDisconnected+= OnDisconnectHandler;
            client.OnData += OnDataHandler;
            client.Connect(ip,port);
        }
        public void SendMessage(byte[] data)
        {
            if (!IsConnect)
                return;
            var arraySegment = new ArraySegment<byte>(data);
            client.Send(arraySegment);
        }
        public void Disconnect()
        {
            client.Disconnect();
        }
        public void TickRefresh()
        {
            client.Tick(100);
        }
        public static T Create<T>(RPCClient client) where T : IService<T>
        {
            return DynamicProxyFactory.CreateDynamicProxy<T>(client);
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
        void OnDataHandler(ArraySegment<byte> arrSeg)
        {
            var rcvLen = arrSeg.Count;
            var rcvData = new byte[rcvLen];
            Array.Copy(arrSeg.Array, arrSeg.Offset, rcvData, 0, rcvLen);
            try
            {
                var type = (RPCPackageType)rcvData[0];
                var data = new byte[rcvLen - 1];
                Array.Copy(rcvData, 1, data, 0, rcvLen - 1);
                switch (type)
                {
                    case RPCPackageType.Fullpackage:
                        {
                            methodsProxy.InvokeRsp(data);
                        }
                        break;
                    case RPCPackageType.Segment:
                        {
                            methodsProxy.InvokeRspSegment(data);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Utility.Debug.LogError(e);
            }
        }
    }
}
