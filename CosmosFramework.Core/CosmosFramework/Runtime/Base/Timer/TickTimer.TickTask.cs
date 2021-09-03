using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    public partial class TickTimer
    {
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
    }
}
