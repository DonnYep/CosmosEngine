using System;
using System.Diagnostics;
using Cosmos;

namespace CosmosEngine
{
    public class Application
    {
        /// <summary>
        /// 获取应用消耗；
        /// </summary>
        /// <returns></returns>
        public static string GetAppProcessUsage()
        {
            using (var myProcess = Process.GetCurrentProcess())
            {
                myProcess.Refresh();
                var str = string.Empty;
                str += "\n";
                str += $"{DateTime.Now} [ - ] {myProcess.ProcessName} \n";
                str += "================================================\n";
                str += $"  Physical memory usage     : {Utility.Converter.FormatBytesSize(myProcess.WorkingSet64)}\n";
                str += $"  Base priority             : {myProcess.BasePriority}\n";
                str += $"  Priority class            : {myProcess.PriorityClass} \n";
                str += $"  User processor time       : {myProcess.UserProcessorTime} \n";
                str += $"  Privileged processor time : {myProcess.PrivilegedProcessorTime}  \n";
                str += $"  Total processor time      : {myProcess.TotalProcessorTime} \n";
                str += $"  Paged system memory size  : {Utility.Converter.FormatBytesSize(myProcess.PagedSystemMemorySize64)} \n";
                str += $"  Paged memory size         : {Utility.Converter.FormatBytesSize(myProcess.PagedMemorySize64)}  \n";
                str += "================================================\n";
                return str;
            }
        }

    }
}
