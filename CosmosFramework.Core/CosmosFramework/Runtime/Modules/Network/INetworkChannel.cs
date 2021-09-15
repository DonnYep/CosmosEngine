using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using System.Configuration;

namespace Cosmos.Network
{
    //================================================
    //1、网络通道指当前local实例与remote进行建立网络连接的一个消息通道；
    //
    //2、当通道为sever时，则此通道作为服务器，管理进入的链接对象；
    //
    //3、当通道为clinet时，则通道作为client对remote进行链接。
    //
    //4、允许存在多通道，通道与通道之间通讯需要自定义实现；
    //================================================
    /// <summary>
    /// 网络通道；
    /// </summary>
    public interface INetworkChannel
    {
        event Action<NetworkChannelKey, int> OnConnected;
        event Action<NetworkChannelKey, int> OnDisconnected;
        event Action<NetworkChannelKey, int,byte[]> OnReceiveData;
        NetworkChannelKey NetworkChannelKey { get; }
        NetworkProtocol NetworkProtocol { get; }
        bool IsConnect { get; }
        void Connect(string ip, ushort port);
        void SendMessage(NetworkReliableType reliableType, byte[] data, int connectionId);
        void Disconnect(int connectionId);
        void AbortChannel();
        void TickRefresh();
    }
}