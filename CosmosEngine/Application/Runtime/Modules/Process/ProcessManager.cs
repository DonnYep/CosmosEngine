using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
using System.Diagnostics;
namespace CosmosEngine
{
    [Module]
    public class ProcessManager : Module, IProcessManager
    {
        readonly long intervalMS = 60000;
        long latesetTime = 0;
        protected override void OnPreparatory()
        {
            latesetTime = Utility.Time.MillisecondNow();
        }
        [TickRefresh]
        void TickRefresh()
        {
            var now = Utility.Time.MillisecondNow();
            if (now >= latesetTime)
            {
                latesetTime =now+ intervalMS;
                PrintProcessInfo();
            }
        }
        void PrintProcessInfo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Process myProcess = Process.GetCurrentProcess();
            myProcess.Refresh();
            Console.WriteLine("\n");
            Console.WriteLine($"{DateTime.Now} [ - ] {myProcess.ProcessName} ");
            Console.WriteLine("================================================");
            Console.WriteLine($"  Physical memory usage     : {Utility.Converter.FormatBytesSize(myProcess.WorkingSet64)}");
            Console.WriteLine($"  Base priority             : {myProcess.BasePriority}");
            Console.WriteLine($"  Priority class            : {myProcess.PriorityClass}");
            Console.WriteLine($"  User processor time       : {myProcess.UserProcessorTime}");
            Console.WriteLine($"  Privileged processor time : {myProcess.PrivilegedProcessorTime}");
            Console.WriteLine($"  Total processor time      : {myProcess.TotalProcessorTime}");
            Console.WriteLine($"  Paged system memory size  : {Utility.Converter.FormatBytesSize(myProcess.PagedSystemMemorySize64)}");
            Console.WriteLine($"  Paged memory size         : {Utility.Converter.FormatBytesSize(myProcess.PagedMemorySize64)}");
            Console.WriteLine("================================================\n");
            Console.ResetColor();
        }
    }
}
