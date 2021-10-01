using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cosmos;
using System.Runtime.InteropServices;
namespace CosmosEngine
{
    public class ServerLauncher
    {
        delegate bool ControlCtrlDelegate(int CtrlType);
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);
        static ControlCtrlDelegate consoleDelegate = new ControlCtrlDelegate(HandlerRoutine);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int hConsoleHandle);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint mode);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint mode);

        public static void Main(string[] args)
        {
            Console.Title = "Server";
            SetConsoleCtrlHandler(consoleDelegate, true);
            DisbleQuickEditMode();
            EngineEntry.LaunchAppDomainHelpers();
            EngineEntry.LaunchAppDomainModules();
            EngineEntry.Run();
        }
        static bool HandlerRoutine(int CtrlType)
        {
            Utility.Debug.LogWarning("Server Shutdown !");//按控制台关闭按钮关闭 
            return false;
        }
        static void DisbleQuickEditMode()
        {
            IntPtr hStdin = GetStdHandle(STD_INPUT_HANDLE);
            uint mode;
            GetConsoleMode(hStdin, out mode);
            mode &= ~ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(hStdin, mode);
        }
    }
}
