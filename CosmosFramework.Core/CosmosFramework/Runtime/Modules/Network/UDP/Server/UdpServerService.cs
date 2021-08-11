using Cosmos.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Cosmos
{
    public class UdpServerService : UdpService
    {
        /// <summary>
        /// 轮询委托
        /// </summary>
        Action clientPeerRefreshHandler;
        public event Action RefreshHandler
        {
            add { clientPeerRefreshHandler += value; }
            remove { clientPeerRefreshHandler -= value; }
        }
        ConcurrentDictionary<long, IRemotePeer> peerDict;


        public override void OnInitialization()
        {
            base.OnInitialization();
            peerDict = new ConcurrentDictionary<long, IRemotePeer>();
        }
        public override async void SendMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            IRemotePeer tmpPeer;
            if (peerDict.TryGetValue(netMsg.Conv, out tmpPeer))
            {
                UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
                UdpClientPeer peer = tmpPeer as UdpClientPeer;
                var result = peer.EncodeMessage(ref udpNetMsg);
                if (result)
                {
                    if (udpSocket != null)
                    {
                        try
                        {
                            var buffer = udpNetMsg.GetBuffer();
                            int length = await udpSocket.SendAsync(buffer, buffer.Length, endPoint);
                            if (length != buffer.Length)
                            {
                                //消息未完全发送，则重新发送
                                SendMessageAsync(udpNetMsg, endPoint);
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"Send net message exceotion :{e.Message}");
                        }
                    }
                }
            }
        }
        public override async void SendMessageAsync(INetworkMessage netMsg)
        {
            IRemotePeer tmpPeer;
            if (peerDict.TryGetValue(netMsg.Conv, out tmpPeer))
            {
                UdpClientPeer peer = tmpPeer as UdpClientPeer;
                UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
                var result = peer.EncodeMessage(ref udpNetMsg);
                if (result)
                {
                    if (udpSocket != null)
                    {
                        try
                        {
                            var buffer = udpNetMsg.GetBuffer();
                            int length = await udpSocket.SendAsync(buffer, buffer.Length, peer.PeerEndPoint);
                            if (length != buffer.Length)
                            {
                                //消息未完全发送，则重新发送
                                SendMessageAsync(udpNetMsg);
                            }
                        }
                        catch (Exception e)
                        {
                            Utility.Debug.LogError($"Send net message exceotion : {e.Message}");
                        }
                    }
                }
            }
        }
        public async override void SendMessageAsync(byte[] buffer, IPEndPoint endPoint)
        {
            if (udpSocket != null)
            {
                try
                {
                    int length = await udpSocket.SendAsync(buffer, buffer.Length, endPoint);
                    if (length != buffer.Length)
                    {
                        //消息未完全发送，则重新发送
                        SendMessageAsync(buffer, endPoint);
                        Utility.Debug.LogInfo($"Send net KCP_ACK message");
                    }
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"Send net message exceotion : {e.Message}");
                }
            }
        }
        /// <summary>
        /// 移除失效peer；
        /// 作为参数传入peer；
        /// </summary>
        /// <param name="conv">会话ID</param>
        public override void AbortUnavilablePeer(IRemotePeer peer)
        {
            try
            {
                peerDict.TryRemove(peer.Conv, out _);
                onDisconnected?.Invoke(peer.Conv);
                Utility.Debug.LogWarning($" Conv :{ conv}  is Unavailable，abort peer ");
                ReferencePool.Release(peer);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError($"remove Unavailable peer fail {e}");
            }
        }
        public override void OnRefresh()
        {
            clientPeerRefreshHandler?.Invoke();
            if (awaitHandle.Count > 0)
            {
                UdpReceiveResult data;
                if (awaitHandle.TryDequeue(out data))
                {
                    UdpNetMessage netMsg = ReferencePool.Accquire<UdpNetMessage>();
                    netMsg.DecodeMessage(data.Buffer);
#if DEBUG
                    if (netMsg.Cmd == UdpProtocol.MSG)
                        Utility.Debug.LogInfo($" OnRefresh KCP_MSG：{netMsg} ;ServiceMessage : {Utility.Converter.GetString(netMsg.ServiceData)},TS:{netMsg.TS}");
#endif
                    if (netMsg.IsFull)
                    {
                        if (netMsg.Conv == 0)
                        {
                            conv += 1;
                            netMsg.Conv = conv;
                            UdpClientPeer peer;
                            CreateClientPeer(netMsg, data.RemoteEndPoint, out peer);
                        }
                        if (peerDict.TryGetValue(netMsg.Conv, out var rPeer))
                        {
                            UdpClientPeer tmpPeer = rPeer as UdpClientPeer;
                            //如果peer失效，则移除
                            if (!tmpPeer.Available)
                            {
                                clientPeerRefreshHandler -= tmpPeer.OnRefresh;
                                AbortUnavilablePeer(rPeer);
                            }
                            else
                            {
                                tmpPeer.MessageHandler(netMsg);
                            }
                        }
                        else
                        {
                            //发送终结命令；
                            UdpNetMessage finMsg = UdpNetMessage.EncodeMessage(netMsg.Conv);
                            finMsg.Cmd = UdpProtocol.FIN;
                            SendFINMessageAsync(finMsg, data.RemoteEndPoint);
                        }
                    }
                }
            }
        }
        async void SendFINMessageAsync(INetworkMessage netMsg, IPEndPoint endPoint)
        {
            UdpNetMessage udpNetMsg = netMsg as UdpNetMessage;
            udpNetMsg.TS = Utility.Time.MillisecondTimeStamp();
            udpNetMsg.EncodeMessage();
            if (udpSocket != null)
            {
                try
                {
                    var buffer = udpNetMsg.GetBuffer();
                    int length = await udpSocket.SendAsync(buffer, buffer.Length, endPoint);
                    if (length != buffer.Length)
                    {
                        //消息未完全发送，则重新发送
                        SendFINMessageAsync(udpNetMsg, endPoint);
                    }
                }
                catch (Exception e)
                {
                    Utility.Debug.LogError($"Send net message exceotion:{e.Message}");
                }
            }
        }

        bool CreateClientPeer(UdpNetMessage udpNetMsg, IPEndPoint endPoint, out UdpClientPeer peer)
        {
            peer = default;
            bool result = false;
            if (!peerDict.ContainsKey(udpNetMsg.Conv))
            {
                peer = ReferencePool.Accquire<UdpClientPeer>();
                peer.SetValue(this, onReceiveData, SendMessageAsync, udpNetMsg.Conv, endPoint);
                result = peerDict.TryAdd(udpNetMsg.Conv, peer);
                if (result)
                {
                    onConnected?.Invoke(udpNetMsg.Conv);
                    clientPeerRefreshHandler += peer.OnRefresh;
                }
            }
            return result;
        }
    }
}
