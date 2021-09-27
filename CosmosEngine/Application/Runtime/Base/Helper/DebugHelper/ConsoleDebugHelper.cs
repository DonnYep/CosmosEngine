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
        readonly string defaultLogPath = Path.GetFullPath(".");
        /// <summary>
        /// 默认构造，使用默认地址与默认log名称
        /// </summary>
        public ConsoleDebugHelper()
        {
            this.logFullPath = Utility.IO.WebPathCombine(defaultLogPath, "Server.log");
            LogInfo("Log file path : " + logFullPath, null);
            AppendWriteTextFile(logFullPath, "Head");
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
        }
        public ConsoleDebugHelper(string logFullPath)
        {
            LogInfo("Log file path : " + logFullPath, null);
            Utility.Text.IsStringValid(logFullPath, "LogFullPath is invalid !");
            this.logFullPath = logFullPath;
            AppendWriteTextFile(logFullPath, "Head");
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
        }
        public void LogInfo(object msg, object context)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{DateTime.Now} [ - ] > INFO : {msg};{context}");
            Info($"{msg};{context}");
            Console.ResetColor();
        }
        /// <summary>
        /// log日志；
        /// </summary>
        /// <param name="msg">消息体</param>
        /// <param name="msgColor">消息颜色</param>
        /// <param name="context">内容，可传递对象</param>
        public void LogInfo(object msg, string msgColor, object context)
        {
            var now = DateTime.Now;
            ConsoleColor color = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), msgColor);
            Console.ForegroundColor = color;
            Console.WriteLine($"{now} [ - ] > INFO : {msg};{context}");
            Info($"{msg};{context}");
            Console.ResetColor();
        }
        public void LogWarning(object msg, object context)
        {
            var now = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{now} [ - ] > WARN : {msg};{context}");
            Warring(msg.ToString());
            Console.ResetColor();
        }
        public void LogError(object msg, object context)
        {
            var now = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"{now} [ - ] > ERROR : {msg};{context}");
            Error($"{msg};{context}");
            Console.ResetColor();
        }
        /// <summary>
        /// 会导致程序崩溃的log
        /// </summary>
        public void LogFatal(object msg, object context)
        {
            var now = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{now} [ - ] > FATAL : {msg};{context}");
            Fatal($"{msg};{context}");
            Console.ResetColor();
        }
        void Error(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now} [ - ] > ERROR : {msg}\nStackTrace[ - ] ：\n{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()} [ - ] > ERROR : {msg}\nStackTrace[ - ] ：\n{st}{st0}{st1}";
#endif
            AppendWriteTextFile(logFullPath, str);
        }
        void Info(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now} [ - ] > INFO : {msg}\nStackTrace[ - ] ：\n{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()} [ - ] > INFO : {msg}\nStackTrace[ - ] ：\n{st}{st0}{st1}";
#endif
            AppendWriteTextFile(logFullPath, str);
        }
        void Warring(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now} [ - ] > WARN : {msg}\nStackTrace[ - ] ：\n{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()} [ - ] > WARN : {msg}\nStackTrace[ - ] ：\n{st}{st0}{st1}";
#endif
            AppendWriteTextFile(logFullPath, str);
        }
        void Fatal(string msg)
        {
#if DEBUG
            StackTrace st = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now} [ - ] > FATAL : {msg}\nStackTrace[ - ] ：\n{st}";
#else
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace st0 = new StackTrace(new StackFrame(3, true));
            StackTrace st1 = new StackTrace(new StackFrame(4, true));
            string str = $"{DateTime.Now.ToString()} [ - ] > FATAL : {msg}\nStackTrace[ - ] ：\n{st}{st0}{st1}";
#endif
            AppendWriteTextFile(logFullPath, str);
        }
        void AppendWriteTextFile(string fileFullPath, string context)
        {
            using (FileStream stream = new FileStream(fileFullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                stream.Position = stream.Length;
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.WriteLine(context);
                    writer.Flush();
                }
            }
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
