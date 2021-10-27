using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;

namespace Cosmos.RPC
{
    internal class RpcSubpackageProcesser
    {
        //TODO需要优化subpackDict Queue池生成；

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
        public void AddFullpackage(int conv, long rpcDataId, byte[] srcData)
        {
            if (!subpackDict.ContainsKey(conv))
            {
                subpackDict.TryAdd(conv, new Queue<RPCDataSegment>());
            }
            //分包
            var segQueue = subpackDict[conv];
            var srcDataLength = srcData.Length;
            var segCount = srcDataLength / RPCConstants.MaxRpcPackSize;
            for (int i = 0; i <= segCount; i++)
            {
                var remainLength = srcDataLength - i * RPCConstants.MaxRpcPackSize;
                byte[] dstData;
                if (remainLength < RPCConstants.MaxRpcPackSize)
                    dstData = new byte[remainLength];
                else
                    dstData = new byte[RPCConstants.MaxRpcPackSize];
                Array.Copy(srcData, i * dstData.Length, dstData, 0, dstData.Length);
                var rpcSeg = new RPCDataSegment(rpcDataId, srcData.Length, dstData);
                segQueue.Enqueue(rpcSeg);
            }
        }
        void SendSubpackage()
        {
            foreach (var subpack in subpackDict)
            {
                var rpcSeg = subpack.Value.Dequeue();
                var rpcSegBytes = RPCDataSegment.Serialize(rpcSeg);
                var rpcSubpackageBytes = new byte[rpcSegBytes.Length + 1];
                rpcSubpackageBytes[0] = (byte)RPCDataPackageType.Subpackage;
                Array.Copy(rpcSegBytes, 0, rpcSubpackageBytes, 1, rpcSegBytes.Length);

                sendMessage(subpack.Key, rpcSubpackageBytes);
                if (subpack.Value.Count == 0)
                    removeSet.Add(subpack.Key);
            }
            foreach (var id in removeSet)
            {
                subpackDict.Remove(id);
            }
            removeSet.Clear();
        }
    }
}
