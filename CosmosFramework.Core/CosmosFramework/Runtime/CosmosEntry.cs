using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cosmos
{
    /// <summary>
    /// Cosmos服务端入口；
    /// </summary>
    public class CosmosEntry 
    {
        public static bool IsPause { get; private set; }
        public static bool Pause
        {
            get { return IsPause; }
            set
            {
                if (IsPause == value)
                    return;
                IsPause = value;
                if (IsPause)
                {
                    OnPause();
                }
                else
                {
                    OnUnPause();
                }
            }
        }
        public static IFSMManager FSMManager { get { return GameManager.GetModule<IFSMManager>(); } }
        public static IConfigManager ConfigManager { get { return GameManager.GetModule<IConfigManager>(); } }
        public static INetworkManager NetworkManager { get { return GameManager.GetModule<INetworkManager>(); } }
        public static IEventManager EventManager { get { return GameManager.GetModule<IEventManager>(); } }
        /// <summary>
        /// 启动当前AppDomain下的helper
        /// </summary>
        public static void LaunchAppDomainHelpers()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var length = assemblies.Length;
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.Debug.IDebugHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.Debug.SetHelper(helper);
                    break;
                }
            }
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.Json.IJsonHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.Json.SetHelper(helper);
                    break;
                }
            }
            for (int i = 0; i < length; i++)
            {
                var helper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.MessagePack.IMessagePackHelper>(assemblies[i]);
                if (helper != null)
                {
                    Utility.MessagePack.SetHelper(helper);
                    break;
                }
            }
        }
        /// <summary>
        /// 启动目标程序集下的helper
        /// </summary>
        /// <param name="assembly">查询的目标程序集</param>
        public static void LaunchAssemblyHelpers(System.Reflection.Assembly assembly)
        {
            var debugHelper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.Debug.IDebugHelper>(assembly);
            if (debugHelper != null)
            {
                Utility.Debug.SetHelper(debugHelper);
            }
            var jsonHelper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.Json.IJsonHelper>(assembly);
            if (jsonHelper != null)
            {
                Utility.Json.SetHelper(jsonHelper);
            }
            var mpHelper = Utility.Assembly.GetInstanceByAttribute<ImplementerAttribute, Utility.MessagePack.IMessagePackHelper>(assembly);
            if (mpHelper != null)
            {
                Utility.MessagePack.SetHelper(mpHelper);
            }
        }
        /// <summary>
        /// 初始化当前AppDomain下的Module；
        /// 注意：初始化尽量只执行一次！！！
        /// </summary>
        public static void LaunchAppDomainModules()
        {
            GameManager.InitAppDomainModule();
        }
        /// <summary>
        /// 初始化目标程序集下的Module；
        /// 注意：初始化尽量只执行一次！！！
        /// </summary>
        /// <param name="assemblies">查询的目标程序集</param>
        public static void LaunchAssemblyModules(params System.Reflection.Assembly[] assemblies)
        {
            GameManager.InitAssemblyModule(assemblies);
        }
        public static void ReleaseLaunchedModules()
        {
            GameManager.Dispose();
        }
        public  static void Run()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1);
                    if (!IsPause)
                        GameManager.OnRefresh();
                }
                catch (System.Exception e)
                {
                    Utility.Debug.LogError(e);
                }
            }
        }
        public static void OnPause()
        {
            GameManager.OnPause();
        }
        public static void OnUnPause()
        {
            GameManager.OnUnPause();
        }
    }
}