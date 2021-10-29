﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using Cosmos;
namespace Cosmos.RPC
{
    internal class RPCTaskService : ConcurrentSingleton<RPCTaskService>
    {
        ConcurrentDictionary<long, IRpcTask> rpcTaskDict;
        ConcurrentQueue<long> removeIdQueue;
        public RPCTaskService()
        {
            rpcTaskDict = new ConcurrentDictionary<long, IRpcTask>();
            removeIdQueue = new ConcurrentQueue<long>();
            CosmosEntry.TickRefreshHandler += TickRefresh;
        }
        public bool AddTask(IRpcTask rpcTask)
        {
            return rpcTaskDict.TryAdd(rpcTask.TaskId, rpcTask);
        }
        public bool RemoveTask(long taskId)
        {
            return rpcTaskDict.TryRemove(taskId, out _);
        }
        public bool PeekTask(long taskId, out IRpcTask rpcTask)
        {
            return rpcTaskDict.TryGetValue(taskId, out rpcTask);
        }
        public override void Dispose()
        {
            rpcTaskDict.Clear();
            removeIdQueue.Clear();
            CosmosEntry.TickRefreshHandler -= TickRefresh;
            base.Dispose();
        }
        public void TickRefresh()
        {
            if (rpcTaskDict.Count == 0)
                return;
            foreach (var task in rpcTaskDict)
            {
                if (task.Value.IsCompleted)
                    removeIdQueue.Enqueue(task.Key);
            }
            if (removeIdQueue.Count > 0)
            {
                var idArray = removeIdQueue.ToArray();
                foreach (var id in idArray)
                {
                    rpcTaskDict.TryRemove(id, out _);
                }
                removeIdQueue.Clear();
            }
        }
    }
}
