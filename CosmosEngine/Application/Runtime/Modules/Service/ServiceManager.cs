using System;
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
        const ushort port = 8566;
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
        KCPServerChannel networkChannel;
        public void SendMessage(byte[] data,int conv)
        {
            networkChannel.SendMessage(conv,data);
        }
        protected override void OnPreparatory()
        {
            networkChannel = new KCPServerChannel("LocksetpServer",  port);
            networkChannel.OnConnected += OnConnectedHandle;
            networkChannel.OnDisconnected += OnDisconnectedHandle;
            networkChannel.OnDataReceived+= OnReceiveDataHandle;
            networkChannel.StartServer();
            CosmosEntry.NetworkManager.AddChannel(networkChannel);
            Utility.Debug.LogInfo(networkChannel.ChannelName+ " Start Running");
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
