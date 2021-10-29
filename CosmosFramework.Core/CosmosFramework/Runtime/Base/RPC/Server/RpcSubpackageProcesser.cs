using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
namespace Cosmos.RPC
{
    internal class RpcSubpackageProcesser
    {
        static readonly ConcurrentPool<Queue<RPCDataSegment>> segmentQueuePool;
        static RpcSubpackageProcesser()
        {
            segmentQueuePool = new ConcurrentPool<Queue<RPCDataSegment>>
                (() => { return new Queue<RPCDataSegment>(); });
        }
        ConcurrentDictionary<int, Queue<RPCDataSegment>> subpackDict;
        HashSet<int> removeSet;
        Action<int, byte[]> sendMessage;
        long latestTime;
        public RpcSubpackageProcesser(Action<int, byte[]> sendMessage)
        {
            this.sendMessage = sendMessage;
            subpackDict = new ConcurrentDictionary<int, Queue<RPCDataSegment>>();
            removeSet = new HashSet<int>();
            latestTime = Utility.Time.MillisecondNow();
        }
        public void TickRefresh()
        {
            if (subpackDict.Count <= 0)
                return;
            var now = Utility.Time.MillisecondNow();
            if (now >= latestTime)
            {
                latestTime = now + RPCConstants.RpcSubpackageSendMSInterval;
                SendSubpackage();
            }
        }
        /// <summary>
        ///添加一个超过最大发送值的二进制包； 
        /// </summary>
        public void AddFullpackage(int conv, RPCData rpcData)
        {
            if (!subpackDict.ContainsKey(conv))
            {
                var segQue = segmentQueuePool.Spawn();
                subpackDict.TryAdd(conv, segQue);
            }
            //分包
            var segQueue = subpackDict[conv];

            var srcSegData = rpcData.ReturnData.Value;
            var srcSegDataLength = rpcData.ReturnData.Value.Length;

            var segCount = srcSegDataLength / RPCConstants.MaxRpcPackSize;
            var remain = srcSegDataLength % RPCConstants.MaxRpcPackSize;

            for (int i = 0; i < segCount; i++)
            {
                byte[] dstData = new byte[RPCConstants.MaxRpcPackSize];
                Array.Copy(srcSegData, i * dstData.Length, dstData, 0, dstData.Length);
                var rpcSeg = new RPCDataSegment(rpcData.RpcDataId, srcSegDataLength, dstData);
                segQueue.Enqueue(rpcSeg);
            }
            if (remain > 0)
            {
                byte[] dstData = new byte[remain];
                Array.Copy(srcSegData, segCount * RPCConstants.MaxRpcPackSize, dstData, 0, dstData.Length);
                var rpcSeg = new RPCDataSegment(rpcData.RpcDataId, srcSegDataLength, dstData);
                segQueue.Enqueue(rpcSeg);
            }
        }
        void SendSubpackage()
        {
            foreach (var subpack in subpackDict)
            {
                var rpcSeg = subpack.Value.Dequeue();
                var rpcSegBytes = RPCDataSegment.Serialize(rpcSeg);

                var sndRpcSegBytes = new byte[rpcSegBytes.Length + 1];

                sndRpcSegBytes[0] = (byte)RPCDataPackageType.Subpackage;

                Array.Copy(rpcSegBytes, 0, sndRpcSegBytes, 1, rpcSegBytes.Length);
                sendMessage(subpack.Key, sndRpcSegBytes);
                if (subpack.Value.Count == 0)
                    removeSet.Add(subpack.Key);
            }
            foreach (var id in removeSet)
            {
                subpackDict.Remove(id, out var queue);
                segmentQueuePool.Despawn(queue);
            }
            removeSet.Clear();
        }
    }
}
