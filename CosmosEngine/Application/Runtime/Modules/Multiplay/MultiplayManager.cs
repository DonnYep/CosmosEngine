using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosEngine
{
    //================================================
    /*
    *1、多人同步模块负责收集玩家输入，定时广播；
    *
    *2、此模块线程安全；
    */
    //================================================
    [Module]
    public class MultiplayManager : Module, IMultiplayManager
    {
        public const int MaxConnection = 32;
        Dictionary<int, MultiplayConnection> connDict;
        List<MultiplayConnection> connList;
        Action<byte[], int> sendMessage;
        /// <summary>
        /// 帧率；
        /// </summary>
        public const int FrameRate = 10;
        /// <summary>
        /// 帧间隔；毫秒；
        /// </summary>
        int Interval;
        long latestTime;
        /// <summary>
        /// key为Conv，value为input数据；
        /// </summary>
        Dictionary<int, List<byte[]>> frameInputDataDict;
        protected override void OnPreparatory()
        {
            connDict = new Dictionary<int, MultiplayConnection>();
            connList = new List<MultiplayConnection>();
            frameInputDataDict = new Dictionary<int, List< byte[]>>();
            Interval = 1000 / FrameRate;
            latestTime = Utility.Time.MillisecondNow() + Interval;

            ServerEntry.ServiceManager.OnReceiveData += OnReceiveDataHandler;
            ServerEntry.ServiceManager.OnConnected += OnConnect;
            ServerEntry.ServiceManager.OnDisconnected += OnDisconnect;
            sendMessage = ServerEntry.ServiceManager.SendMessage; ;
        }
        [TickRefresh]
        void OnRefresh()
        {
            if (IsPause)
                return;
            var msNow = Utility.Time.MillisecondNow();
            if (latestTime <= msNow)
            {
                latestTime = msNow + Interval;
                var length = connList.Count;
                for (int i = 0; i < length; i++)
                {
                    frameInputDataDict[connList[i].Conv].Clear();
                    frameInputDataDict[connList[i].Conv].AddRange(connList[i].TransportData);
                    connList[i].TransportData.Clear();
                }
                var data = Utility.MessagePack.Serialize(frameInputDataDict);
                var inputOpData = new MultiplayData((byte)MultiplayOperationCode.PlayerInput, 0, 0, data);
                var sndData = MultiplayData.Serialize(inputOpData);
                for (int i = 0; i < length; i++)
                {
                    sendMessage(sndData, connList[i].Conv);
                }
            }
        }
        void OnReceiveDataHandler(int conv, byte[] data)
        {
            try
            {
                var opData = MultiplayData.Deserialize(data);
                ProcessHandler(conv, opData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError($"Conv msg error {e}");
            }
        }
        void OnConnect(int conv)
        {
            MultiplayData opData = new MultiplayData();
            if (MaxConnection >= connDict.Count)
            {
                PlayerEnter(conv);
                //连接成功后，将自己的conv与已经存在的conv返回；
                Utility.Debug.LogWarning($"conv {conv} 建立连接；当前连接数 : {connDict.Count}, 最大连接数 :{MaxConnection}");
                opData.OperationCode = (byte)MultiplayOperationCode.SYN;
                var messageDict = new Dictionary<byte, object>();
                var remoteConvs = new List<int>();
                remoteConvs.AddRange(connDict.Keys.ToList());

                messageDict.Add((byte)MultiplayParameterCode.AuthorityConv, conv);
                messageDict.Add((byte)MultiplayParameterCode.RemoteConvs, Utility.Json.ToJson(remoteConvs));
                messageDict.Add((byte)MultiplayParameterCode.ServerSyncInterval, Interval);

                opData.DataContract = Utility.MessagePack.Serialize(messageDict);
                var conn = new MultiplayConnection() { Conv = conv };
                connList.Add(conn);
                connDict.TryAdd(conv, conn);
            }
            else
            {
                opData.OperationCode = (byte)MultiplayOperationCode.FIN;
                opData.DataContract = Encoding.UTF8.GetBytes($"当前案例场景最大连接数为:{MaxConnection}，已超出最大连接数，服务器对此进行断开连接操作。若需要更改最大连接数，请修改服务器 MovementSphereManager.MaxConnection的数量");
            }
            var data = MultiplayData.Serialize(opData);
            sendMessage(data, conv);
        }
        void OnDisconnect(int conv)
        {
            if (connDict.Remove(conv, out var conn))
            {
                connList.Remove(conn);
                MultiplayData opData = new MultiplayData();
                opData.OperationCode = (byte)MultiplayOperationCode.PlayerExit;
                opData.DataContract = BitConverter.GetBytes(conv);
                BroadCastMessage(opData);
                frameInputDataDict.Remove(conv);
                Utility.Debug.LogWarning($"conv {conv} 断开连接；当前连接数 : {connDict.Count},");
            }
        }
        void BroadCastMessage(MultiplayData opData)
        {
            var data = MultiplayData.Serialize(opData);
            foreach (var conn in connDict)
            {
                sendMessage(data, conn.Key);
            }
        }
        void PlayerEnter(int conv)
        {
            MultiplayData opData = new MultiplayData();
            opData.OperationCode = (byte)MultiplayOperationCode.PlayerEnter;
            opData.DataContract = BitConverter.GetBytes(conv);
            var data = MultiplayData.Serialize(opData);
            frameInputDataDict[conv] = new List<byte[]>();
            foreach (var conn in connDict)
            {
                sendMessage(data, conn.Key);
            }
        }
        void ProcessHandler(int conv, MultiplayData opData)
        {
            var opCode = (MultiplayOperationCode)opData.OperationCode;
            switch (opCode)
            {
                case MultiplayOperationCode.PlayerInput:
                    {
                        if (connDict.TryGetValue(conv, out var conn))
                        {
                            conn.TransportData.Add(opData.DataContract);
                            Utility.Debug.LogInfo($"conv{conv} send input data");
                        }
                    }
                    break;
            }
        }
    }
}
