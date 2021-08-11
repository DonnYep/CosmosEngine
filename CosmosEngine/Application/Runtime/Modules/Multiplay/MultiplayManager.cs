using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace CosmosEngine
{
    /// <summary>
    /// 对应unity network 案例部分；
    /// </summary>
    [Module]
    public class MultiplayManager : Module, IMultiplayManager
    {
        public const int MaxConnection = 5;
        Dictionary<int, Connection> connDict;
        List<Connection> connList;
        /// <summary>
        /// 帧率；
        /// </summary>
        public const int FrameRate = 20;
        /// <summary>
        /// 帧间隔；毫秒；
        /// </summary>
        int Interval;
        long latestTime;
        /// <summary>
        /// key为Conv，value为input数据；
        /// </summary>
        List<FixTransportData> frameInputData;
        /// <summary>
        /// 当前帧;
        /// </summary>
        long currentFrame = 0;
        OperationData inputOpData;
        protected override void OnPreparatory()
        {
            connDict = new Dictionary<int, Connection>();
            connList = new List<Connection>();
            frameInputData = new List<FixTransportData>();
            inputOpData = new OperationData((byte)MultiplayOperationCode.PlayerInput);
            Interval = (int)1000 / FrameRate;
            latestTime = Utility.Time.MillisecondNow() + Interval;
            CosmosEntry.NetworkManager.OnReceiveData += OnReceiveDataHandler;
            CosmosEntry.NetworkManager.OnDisconnected += OnDisconnect;
            CosmosEntry.NetworkManager.OnConnected += OnConnect;
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
                frameInputData.Clear();
                var length = connList.Count;
                for (int i = 0; i < length; i++)
                {
                    frameInputData.Add(connList[i].TransportData);
                }
                inputOpData.DataContract = Utility.Json.ToJson(frameInputData);
                var json = Utility.Json.ToJson(inputOpData);
                var sndData = Encoding.UTF8.GetBytes(json);
                for (int i = 0; i < length; i++)
                {
                    CosmosEntry.NetworkManager.SendNetworkMessage(sndData, connList[i].Conv);
                }
                currentFrame++;
            }
        }
        void OnReceiveDataHandler(int conv, byte[] data)
        {
            try
            {
                var json = Encoding.UTF8.GetString(data);
                var opData = Utility.Json.ToObject<OperationData>(json);
                ProcessHandler(conv, opData);
            }
            catch (Exception e)
            {
                Utility.Debug.LogError($"Conv msg error {e}");
            }
        }
        void OnConnect(int conv)
        {
            OperationData opData = new OperationData();
            byte[] data = null;
            if (MaxConnection >= connDict.Count)
            {
                PlayerEnter(conv);

                //连接成功后，将自己的conv与已经存在的conv返回；
                Utility.Debug.LogWarning($"conv {conv} connected；current Connection count : {connDict.Count},MaxConnection :{MaxConnection}");
                opData.OperationCode = (byte)MultiplayOperationCode.SYN;
                var messageDict = new Dictionary<byte, object>();
                var remoteConvs = new List<int>();
                remoteConvs.AddRange(connDict.Keys.ToList());

                messageDict.Add((byte)MultiplayParameterCode.AuthorityConv, conv);
                messageDict.Add((byte)MultiplayParameterCode.RemoteConvs, Utility.Json.ToJson(remoteConvs));
                messageDict.Add((byte)MultiplayParameterCode.ServerSyncInterval, Interval);

                opData.DataContract = Utility.Json.ToJson(messageDict);
                var json = Utility.Json.ToJson(opData);
                data = Encoding.UTF8.GetBytes(json);
                var conn = new Connection() { Conv = conv };
                connList.Add(conn);
                connDict.TryAdd(conv, conn);
            }
            else
            {
                opData.OperationCode = (byte)MultiplayOperationCode.FIN;
                opData.DataContract = $"当前案例场景最大连接数为:{MaxConnection}，已超出最大连接数，服务器对此进行断开连接操作。若需要更改最大连接数，请修改服务器 MovementSphereManager.MaxConnection的数量";
                var json = Utility.Json.ToJson(opData);
                data = Encoding.UTF8.GetBytes(json);
            }
            CosmosEntry.NetworkManager.SendNetworkMessage(data, conv);
        }
        void OnDisconnect(int conv)
        {
            if (connDict.Remove(conv, out var conn))
            {
                connList.Remove(conn);
                OperationData opData = new OperationData();
                opData.OperationCode = (byte)MultiplayOperationCode.PlayerExit;
                opData.DataContract = conv;
                BroadCastMessage(opData);
                Utility.Debug.LogWarning($"conv {conv} disconnected；current Connection count : {connDict.Count},");
            }
        }
        void BroadCastMessage(OperationData opData)
        {
            var json = Utility.Json.ToJson(opData);
            var data = Encoding.UTF8.GetBytes(json);
            foreach (var conn in connDict)
            {
                CosmosEntry.NetworkManager.SendNetworkMessage(data, conn.Key);
            }
        }
        void PlayerEnter(long conv)
        {
            OperationData opData = new OperationData();
            opData.OperationCode = (byte)MultiplayOperationCode.PlayerEnter;
            opData.DataContract = conv;
            var json = Utility.Json.ToJson(opData);
            var data = Encoding.UTF8.GetBytes(json);
            foreach (var conn in connDict)
            {
                CosmosEntry.NetworkManager.SendNetworkMessage(data, conn.Key);
            }
        }
        void ProcessHandler(int conv, OperationData opData)
        {
            var opCode = (MultiplayOperationCode)opData.OperationCode;
            switch (opCode)
            {
                case MultiplayOperationCode.PlayerInput:
                    {
                        var transportData = Utility.Json.ToObject<FixTransportData>(Convert.ToString(opData.DataContract));
                        if (connDict.TryGetValue(conv, out var conn))
                        {
                            conn.TransportData = transportData;
                        }
                    }
                    break;
            }
        }
    }
}
