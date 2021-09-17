using System;
using System.Collections.Generic;
using System.Text;
using kcp;
using System.Threading;
using Cosmos;
using System.Runtime.InteropServices;
using Cosmos.Network;

namespace CosmosEngine
{
    public class KCPServerLauncher
    {
        static ushort port = 8531;

        delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        static ControlCtrlDelegate consoleDelegate = new ControlCtrlDelegate(HandlerRoutine);

        public static void Main(string[] args)
        {
            Console.Title = "KCPServer";
            SetConsoleCtrlHandler(consoleDelegate, true);
            EngineEntry.LaunchAppDomainHelpers();
            EngineEntry.LaunchAppDomainModules();
            var serverChannel = new KCPServerChannel("KCPServer", "localhost", port);
           var channelKey = serverChannel.NetworkChannelKey;
            EngineEntry.MultiplayManager.SetNetworkChannel(serverChannel);
            serverChannel.Connect();
            EngineEntry.NetworkManager.AddChannel(serverChannel);
            Utility.Debug.LogInfo($"{channelKey} Start Running !");
            EngineEntry.Run();
        }
        static bool HandlerRoutine(int CtrlType)
        {
            Utility.Debug.LogWarning("Server Shutdown !");//按控制台关闭按钮关闭 
            return false;
        }
    }
}
