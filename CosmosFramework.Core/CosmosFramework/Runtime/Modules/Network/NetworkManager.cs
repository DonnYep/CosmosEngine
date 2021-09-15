using System.Collections;
using Cosmos;
using System.Net;
using System.Net.Sockets;
using System;
using System.Diagnostics;
using System.Collections.Concurrent;
using kcp;
using System.Linq;

namespace Cosmos.Network
{
    [Module]
    internal sealed partial class NetworkManager : Module//, INetworkManager
    {
        KcpServerService kcpServerService;
        /// <summary>
        /// ChannelName===INetworkChannel
        /// </summary>
        ConcurrentDictionary<NetworkChannelKey, INetworkChannel> networkChannelDict;

        Action<NetworkChannelKey, int, byte[]> onReceiveData;
        Action<NetworkChannelKey, int> onConnected;
        Action<NetworkChannelKey, int> onDisconnected;
        public event Action<NetworkChannelKey, int, byte[]> OnReceiveData
        {
            add { onReceiveData += value; }
            remove { onReceiveData -= value; }
        }
        public event Action<NetworkChannelKey, int> OnConnected
        {
            add { onConnected += value; }
            remove { onConnected -= value; }
        }
        public event Action<NetworkChannelKey, int> OnDisconnected
        {
            add { onDisconnected += value; }
            remove { onDisconnected -= value; }
        }

        public bool AddChannel(NetworkChannelKey channelKey, INetworkChannel channel)
        {
            if (networkChannelDict.TryAdd(channelKey, channel))
            {
                channel.OnConnected += OnChannelConnected;
                channel.OnDisconnected += OnChannelDisconnected;
                channel.OnReceiveData += OnChannelReceiveData;
                return true;
            }
            return false;
        }
        public bool RemoveChannel(NetworkChannelKey channelKey, out INetworkChannel channel)
        {
            if (networkChannelDict.TryRemove(channelKey, out channel))
            {
                channel.AbortChannel();
                channel.OnConnected += OnChannelConnected;
                channel.OnDisconnected += OnChannelDisconnected;
                channel.OnReceiveData += OnChannelReceiveData;
                return true;
            }
            return false;
        }
        public bool PeekChannel(NetworkChannelKey channelKey, out INetworkChannel channel)
        {
            return networkChannelDict.TryGetValue(channelKey, out channel);
        }
        public bool HasChannel(NetworkChannelKey channelKey)
        {
            return networkChannelDict.ContainsKey(channelKey);
        }
        public INetworkChannel[] PeekAllChannels()
        {
            return networkChannelDict.Values.ToArray();
        }
        public NetworkChannelInfo GetChannelInfo(NetworkChannelKey channelKey)
        {
            if( networkChannelDict.TryGetValue(channelKey, out var channel))
            {
                var info = new NetworkChannelInfo();
                info.NetworkType= channel.NetworkProtocol.ToString();
                info.IPAddress= channel.NetworkChannelKey.ChannelIPAddress;
                info.Name = channel.NetworkChannelKey.ChannelName;
                return info;
            }
            return NetworkChannelInfo.None;
        }
        public void SendNetworkMessage(NetworkChannelKey channelKey, NetworkReliableType reliableType, byte[] data, int connectionId)
        {
            if (networkChannelDict.TryGetValue(channelKey, out var channel))
            {
                channel.SendMessage(reliableType, data, connectionId);
            }
        }
        public void Connect(NetworkChannelKey channelKey, string ip, ushort port)
        {
            if (networkChannelDict.TryGetValue(channelKey, out var channel))
            {
                channel.Connect(ip, port);
            }
        }
        public void Disconnect(NetworkChannelKey channelKey, int connectionId)
        {
            if (networkChannelDict.TryGetValue(channelKey, out var channel))
            {
                channel.Disconnect(connectionId);
            }
        }
        public void AbortChannel(NetworkChannelKey channelKey)
        {
            if (networkChannelDict.TryGetValue(channelKey, out var channel))
            {
                channel.AbortChannel();
            }
        }
        protected override void OnInitialization()
        {
            IsPause = false;
            networkChannelDict = new ConcurrentDictionary<NetworkChannelKey, INetworkChannel>();
        }
        protected override void OnTermination()
        {
            foreach (var channel in networkChannelDict)
            {
                channel.Value.AbortChannel();
            }
            networkChannelDict.Clear();
        }
        [TickRefresh]
        void OnRefresh()
        {
            if (IsPause)
                return;
            //foreach 时不允许操作字典对象，则转换成数组进行操作；
            var channelArr = networkChannelDict.Values.ToArray();
            foreach (var channel in channelArr)
            {
                channel.TickRefresh();
            }
        }

        void OnChannelConnected(NetworkChannelKey channel, int connectionId)
        {
            onConnected.Invoke(channel, connectionId);
        }
        void OnChannelDisconnected(NetworkChannelKey channel, int connectionId)
        {
            onDisconnected.Invoke(channel, connectionId);
        }
        void OnChannelReceiveData(NetworkChannelKey channel, int connectionId, byte[] data)
        {
            onReceiveData.Invoke(channel, connectionId, data);
        }
    }
}
