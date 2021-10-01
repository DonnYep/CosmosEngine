using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using Cosmos.Network;
namespace CosmosEngine
{
    /// <summary>
    /// 服务启动模块；
    /// </summary>
    [Module]
    public class ServiceManager : Module, IServiceManager
    {
        const ushort port = 8531;

        Action<int> onConnected;
        public event Action<int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        Action<int> onDisconnected;
        public event Action<int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }
        Action<int, byte[]> onReceiveData;
        public event Action<int,byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        INetworkChannel networkChannel;
        public void SendMessage(byte[] data,int conv)
        {
            networkChannel.SendMessage(data,conv);
        }
        protected override void OnPreparatory()
        {
            networkChannel = new KCPServerChannel("LocksetpServer", "localhost", port);
            networkChannel.OnConnected += OnConnectedHandle;
            networkChannel.OnDisconnected += OnDisconnectedHandle;
            networkChannel.OnReceiveData += OnReceiveDataHandle;
            EngineEntry.NetworkManager.AddChannel(networkChannel);
            networkChannel.Connect();
            Utility.Debug.LogInfo(networkChannel.NetworkChannelKey + " Start Running");
        }
        void OnReceiveDataHandle(int conv, byte[] data)
        {
            onReceiveData?.Invoke(conv, data);
        }
        void OnDisconnectedHandle(int conv)
        {
            onDisconnected?.Invoke(conv);
        }
        void OnConnectedHandle(int conv)
        {
            onConnected?.Invoke(conv);
        }
    }
}
