using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
namespace Cosmos
{
    public partial class TickTimer
    {
        class TickTaskGroup
        {
            public int TaskId { get; }
            public Action<int> Callbak { get; }
            public TickTaskGroup(int taskId, Action<int> callbak)
            {
                TaskId = taskId;
                Callbak = callbak;
            }
        }
        class TickTask
        {
            public int TaskId { get; set; }
            public uint Delay { get; set; }
            public int Count { get; set; }
            public double DestTime { get; set; }
            public Action<int> TaskCallback { get; set; }
            public Action<int> CancelCallback { get; set; }
            public double StartTime { get; set; }
            public ulong LoopIndex { get; set; }
            public TickTask(int taskId, uint delay, int count, double destTime, Action<int> taskCallback, Action<int> cancelCallback, double startTime)
            {
                this.TaskId = taskId;
                this.Delay = delay;
                this.Count = count;
                this.DestTime = destTime;
                this.TaskCallback = taskCallback;
                this.CancelCallback = cancelCallback;
                this.StartTime = startTime;
            }
        }

        readonly DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        readonly ConcurrentDictionary<int, TickTimer.TickTask> taskDict;
        readonly bool setHandle;
        /// <summary>
        /// 缓存池；
        /// </summary>
        readonly ConcurrentQueue<TickTaskGroup> taskCacheQue;
        const string TaskIdLocker = "TickTimer_TaskIdLocker";
        readonly Thread timerThread;
        int TaskId = 0;

        public Action<string> LogInfo { get; set; }
        public Action<string> LogError { get; set; }
        public Action<string> LogWarn { get; set; }
        public TickTimer(int interval = 0,bool setHandle=false)
        {
            taskDict = new ConcurrentDictionary<int, TickTask>();
            taskCacheQue = new ConcurrentQueue<TickTaskGroup>();
            this.setHandle = setHandle;
            if (interval != 0)
            {
                timerThread = new Thread(new ThreadStart(() => StartTick(interval)));
                timerThread.Start();
            }
        }
        public int AddTask(uint delay, Action<int> taskCallback, Action<int> cancelCallback, int count = 1)
        {
            int tid = GenerateTaskId();
            double startTime = GetUTCMilliseconds();
            double destTime = startTime + delay;
            var task = new TickTask(tid, delay, count, destTime, taskCallback, cancelCallback, startTime);
            if (taskDict.TryAdd(tid, task))
            {
                return tid;
            }
            else
            {
                return -1;
            }
        }
        public bool RemoveTask(int taskId)
        {
            if (taskDict.TryRemove(taskId, out TickTask task))
            {
                if (setHandle && task.CancelCallback != null)
                {
                    taskCacheQue.Enqueue(new TickTaskGroup(taskId, task.CancelCallback));
                }
                else
                {
                    task.CancelCallback?.Invoke(taskId);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public void TickRefresh()
        {
            double nowTime = GetUTCMilliseconds();
            foreach (var item in taskDict)
            {
                TickTask task = item.Value;
                if (nowTime < task.DestTime)
                {
                    continue;
                }
                ++task.LoopIndex;
                if (task.Count > 0)
                {
                    --task.Count;
                    if (task.Count == 0)
                    {
                        FinishTask(task.TaskId);
                    }
                    else
                    {
                        task.DestTime = task.StartTime + task.Delay * (task.LoopIndex + 1);
                        TaskCallback(task.TaskId, task.CancelCallback);
                    }
                }
                else
                {
                    task.DestTime = task.StartTime + task.Delay * (task.LoopIndex + 1);
                    TaskCallback(task.TaskId, task.TaskCallback);
                }
            }
            while (taskCacheQue != null && taskCacheQue.Count > 0)
            {
                if (taskCacheQue.TryDequeue(out var pack))
                {
                    pack.Callbak.Invoke(pack.TaskId);
                }
            }
        }
        public void Reset()
        {
            if (!taskCacheQue.IsEmpty)
            {
                //WarnFunc?.Invoke("Callback Queue is not Empty.");
            }
            taskDict.Clear();
            if (timerThread != null)
            {
                timerThread.Abort();
            }
        }
        int GenerateTaskId()
        {
            lock (TaskIdLocker)
            {
                while (true)
                {
                    ++TaskId;
                    if (TaskId == int.MaxValue)
                    {
                        TaskId = 0;
                    }
                    if (!taskDict.ContainsKey(TaskId))
                    {
                        return TaskId;
                    }
                }
            }
        }
        void StartTick(int interval)
        {
            try
            {
                while (true)
                {
                    TickRefresh();
                    Thread.Sleep(interval);
                }
            }
            catch (ThreadAbortException e)
            {
                throw e;
            }
        }
        void TaskCallback(int taskId, Action<int> taskCallback)
        {
            if (setHandle)
            {
                taskCacheQue.Enqueue(new TickTaskGroup(taskId, taskCallback));
            }
            else
            {
                taskCallback.Invoke(taskId);
            }
        }
        void FinishTask(int tid)
        {
            //线程安全字典，遍历过程中删除无影响。
            if (taskDict.TryRemove(tid, out TickTask task))
            {
                TaskCallback(tid, task.CancelCallback);
                task.CancelCallback = null;
            }
        }
        double GetUTCMilliseconds()
        {
            TimeSpan ts = DateTime.UtcNow - startDateTime;
            return ts.TotalMilliseconds;
        }
    }
}
