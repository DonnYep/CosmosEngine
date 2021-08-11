using System;
using System.Collections.Generic;
using System.Text;
using kcp;
using System.Threading;
using Cosmos;
using System.Runtime.InteropServices;
namespace CosmosEngine
{
   public class CosmosKCPServerLauncher
    {
        static ushort port = 8531;
        public delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);
        public static bool HandlerRoutine(int CtrlType)
        {
            Utility.Debug.LogInfo("Server Shutdown !");//按控制台关闭按钮关闭 
            return false;
        }
        public static void Main(string[] args)
        {
            SetConsoleCtrlHandler(newDelegate, true);
            Cosmos.CosmosEntry.LaunchAppDomainHelpers();
            Cosmos.CosmosEntry.LaunchAppDomainModules();
            Utility.Debug.LogInfo("KCP Server Start Running !");
            CosmosEntry.NetworkManager.Connect(port);
            CosmosEntry.Run();
        }
    }
}
