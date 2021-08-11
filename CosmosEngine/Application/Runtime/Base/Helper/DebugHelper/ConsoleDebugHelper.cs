using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cosmos;
namespace ProtocolCore
{
    [Implementer]
    public class ConsoleDebugHelper : Utility.Debug.IDebugHelper
    {
        readonly string logFullPath;
        readonly string logFileName = "CosmosFrameworkServer.log";
        readonly string logFolderName = "Log";
        readonly string defaultLogPath =
#if DEBUG
            Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
#else
            Directory.GetParent(Environment.CurrentDirectory).FullName;
#endif
        /// <summary>
        /// 默认构造，使用默认地址与默认log名称
        /// </summary>
        public ConsoleDebugHelper()
        {
            logFullPath = Utility.IO.PathCombine(defaultLogPath, logFolderName);
            Utility.IO.CreateFolder(logFullPath);
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            LogInfo("Log file path : " + logFullPath, null);
        }
        public ConsoleDebugHelper(string logName)
        {
            if (string.IsNullOrEmpty(logName))
                logName = logFileName;
            if (logName.EndsWith(".log"))
                logFileName = logName;
            else
                logFileName = Utility.Text.Append(logName, ".log");
            logFullPath = Utility.IO.PathCombine(defaultLogPath, logFolderName);
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            Utility.IO.CreateFolder(logFullPath);
            LogInfo("Log file path : " + logFullPath, null);
        }
        public ConsoleDebugHelper(string logName, string logFullPath)
        {
            if (string.IsNullOrEmpty(logName))
                logName = logFileName;
            if (string.IsNullOrEmpty(logFullPath))
            {
                this.logFullPath = Utility.IO.PathCombine(defaultLogPath, logFolderName);
            }
            else
                this.logFullPath = logFileName;
            Utility.IO.CreateFolder(this.logFullPath);
            LogInfo("Log file path : " + logFullPath, null);
        }
        public void LogInfo(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"LogInfo : { msg};{context}\n");
            Info($"{msg};{context}");
            Console.ResetColor();
        }
        /// <summary>
        /// log日志；
        /// 调用时msgColor参考((int)ConsoleColor.White).ToString(;
        /// </summary>
        /// <param name="msg">消息体</param>
        /// <param name="msgColor">消息颜色</param>
        /// <param name="context">内容，可传递对象</param>
        public void LogInfo(object msg, string msgColor, object context)
        {
            ConsoleColor color = (ConsoleColor)int.Parse(msgColor);
            Console.ForegroundColor = color;
            Console.WriteLine($"INFO: { msg};{context}\n");
            Info($"{msg};{context}");
            Console.ResetColor();
        }
        public void LogWarning(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"WARN : { msg};{context}\n");
            Warring(msg.ToString());
            Console.ResetColor();
        }
        public void LogError(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR : { msg};{context}\n");
            Error($"{msg};{context}");
            Console.ResetColor();
        }
        /// <summary>
        /// 会导致程序崩溃的log
        /// </summary>
        public void LogFatal(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"FATAL : { msg};{context}\n");
            Fatal($"{msg};{context}");
            Console.ResetColor();
        }

        void Error(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > ERROR : {msg};\nStackTrace[ - ] ：\n {st}";
#else

            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > ERROR : {msg};\nStackTrace[ - ] ：\n {st}{st0}{st1}";
#endif
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        void Info(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > INFO : {msg};\nStackTrace[ - ] ：{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > INFO : {msg};\nStackTrace[ - ] ：\n {st}{st0}{st1}";
#endif
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        void Warring(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > WARN : {msg};\nStackTrace[ - ] ：{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > WARN : {msg};\nStackTrace[ - ] ：\n {st}{st0}{st1}";
#endif
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        void Fatal(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > FATAL : {msg};\nStackTrace[ - ] ：{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()}[ - ] > FATAL : {msg};\nStackTrace[ - ] ：\n {st}{st0}{st1}";
#endif
            Utility.IO.AppendWriteTextFile(logFullPath, logFileName, str);
        }
        /// <summary>
        /// 全局异常捕获器
        /// </summary>
        /// <param name="sender">异常抛出者</param>
        /// <param name="e">未被捕获的异常</param>
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Utility.Debug.LogError(e);
        }
    }
}
