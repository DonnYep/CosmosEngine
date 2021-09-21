using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
namespace Cosmos
{
    public partial class TickTimer
    {
        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarn { get; set; }
        public Action<string> LogError { get; set; }

        readonly DateTime startDateTime = new DateTime(1970, 1, 1, 0, 0, 0);
        readonly ConcurrentDictionary<int, TickTask> taskDict;
        readonly object locker = new object();
        readonly Thread timerThread;
        int TaskId = 0;
        int tickInterval;

        public TickTimer(int interval = 0)
        {
            if (interval < 0)
                throw new ArgumentException($"{nameof (interval)} is invalid !" );
            tickInterval = interval;
            taskDict = new ConcurrentDictionary<int, TickTask>();
            if (interval != 0)
            {
                timerThread = new Thread(new ThreadStart(() => RunTick()));
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
                task.CancelCallback?.Invoke(taskId);
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
        }
        public void Reset()
        {
            taskDict.Clear();
            if (timerThread != null)
            {
                timerThread.Abort();
            }
        }
        int GenerateTaskId()
        {
            lock (locker)
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
        void RunTick()
        {
            try
            {
                while (true)
                {
                    TickRefresh();
                    Thread.Sleep(tickInterval);
                }
            }
            catch (ThreadAbortException e)
            {
                LogError(e.ToString());
            }
        }
        void TaskCallback(int taskId, Action<int> taskCallback)
        {
            taskCallback.Invoke(taskId);
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
