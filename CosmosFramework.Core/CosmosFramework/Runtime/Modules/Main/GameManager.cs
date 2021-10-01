﻿using Cosmos.Network;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cosmos
{
    internal static class GameManager
    {
        #region Properties
        /// <summary>
        /// 轮询更新委托
        /// </summary>
        static Action tickRefreshHandler;
        internal static event Action TickRefreshHandler
        {
            add { tickRefreshHandler += value; }
            remove { tickRefreshHandler -= value; }
        }
        /// <summary>
        /// 时间流逝轮询委托；
        /// </summary>
        static Action<long> elapseRefreshHandler;
        internal static event Action<long> ElapseRefreshHandler
        {
            add { elapseRefreshHandler += value; }
            remove { elapseRefreshHandler -= value; }
        }
        internal static bool IsPause { get; private set; }
        internal static System.Reflection.Assembly[] Assemblies { get; private set; }
        //当前注册的模块总数
        static int moduleCount = 0;
        internal static int ModuleCount { get { return moduleCount; } }
        internal static bool PrintModulePreparatory { get; set; }

        /// <summary>
        /// 模块字典；
        /// key=>moduleType；value=>module
        /// </summary>
        static ConcurrentDictionary<Type, Module> moduleDict;
        /// <summary>
        /// 接口-module字典；
        /// key=>IModuleManager；value=>Module
        /// </summary>
        static Dictionary<Type, Type> interfaceModuleDict;

        /// <summary>
        /// 模块轮询字典缓存；
        /// key=>module object ; value=>Action ;
        /// </summary>
        static Dictionary<object, Action> tickRefreshDict;
        /// <summary>
        /// 模块初始化时的异常集合；
        /// </summary>
        static List<Exception> moduleInitExceptionList;
        /// <summary>
        /// 模块终止时的异常集合；
        /// </summary>
        static List<Exception> moduleTerminateExceptionList;
        #endregion
        #region Methods
        /// <summary>
        /// 获取模块；
        /// 若需要进行外部扩展，请继承自Module，需要实现接口 IModuleManager，并标记特性：ModuleAttribute
        /// 如：public class TestManager:Module,ITestManager{}
        /// ITestManager 需要包含所有外部可调用的方法；具体请参考Cosmos源码；
        /// </summary>
        /// <typeparam name="T">内置模块接口</typeparam>
        /// <returns>模板模块接口</returns>
        internal static T GetModule<T>() where T : class, IModuleManager
        {
            Type interfaceType = typeof(T);
            var hasType = interfaceModuleDict.TryGetValue(interfaceType, out var derivedType);
            if (!hasType)
                return null;
            moduleDict.TryGetValue(derivedType, out var module);
            return module as T;
        }
        internal static void Dispose()
        {
            OnDeactive();
        }
        internal static void OnPause()
        {
            IsPause = true;
        }
        internal static void OnUnPause()
        {
            IsPause = false;
        }
        internal static void OnRefresh()
        {
            if (IsPause)
                return;
            tickRefreshHandler?.Invoke();
        }
        /// <summary>
        /// 时间流逝轮询;
        /// </summary>
        /// <param name="msNow">utc毫秒当前时间</param>
        internal static void OnElapseRefresh(long msNow)
        {
            if (IsPause)
                return;
            elapseRefreshHandler?.Invoke(msNow);
        }
        /// <summary>
        /// 终结并释放GameManager
        /// </summary>
        internal static bool HasModule(Type type)
        {
            return moduleDict.ContainsKey(type);
        }
        static void ModuleTermination(Module module)
        {
            var type = module.GetType();
            if (HasModule(type))
            {
                Utility.Assembly.InvokeMethod(module, LifecycleMethodsConstant.OnDeactive);
                var m = moduleDict[type];
                if (tickRefreshDict.Remove(module, out var tickAction))
                    TickRefreshHandler -= tickAction;
                moduleDict.Remove(type);
                moduleCount--;
                Utility.Assembly.InvokeMethod(module, LifecycleMethodsConstant.OnTermination);
                Utility.Debug.LogInfo($"Module :{module} is OnTermination", MessageColor.DARKBLUE);
            }
            else
                throw new ArgumentException($"Module : {module} is not exist!");
        }
        internal static void InitAssemblyModule(System.Reflection.Assembly[] assemblies)
        {
            if (assemblies == null)
                throw new ArgumentNullException("InitAssemblyModule : assemblies is invalid");
            Assemblies = assemblies;
            InitDicts();
            var ModuleManagerType = typeof(IModuleManager);
            var assemblyLength = assemblies.Length;
            for (int h = 0; h < assemblyLength; h++)
            {
                var modules = Utility.Assembly.GetInstancesByAttribute<ModuleAttribute, Module>(assemblies[h]);
                for (int i = 0; i < modules.Length; i++)
                {
                    var type = modules[i].GetType();
                    var module = modules[i];
                    if (ModuleManagerType.IsAssignableFrom(type))
                    {
                        if (!moduleDict.ContainsKey(type))
                        {
                            try
                            {
                                var interfaces = type.GetInterfaces();
                                Type interfaceType = null;
                                foreach (var inter in interfaces)
                                {
                                    if (inter.Name.Contains(type.Name))
                                    {
                                        interfaceType = inter;
                                        break;
                                    }
                                }
                                if (interfaceType != null)
                                {
                                    moduleDict.TryAdd(type, module);
                                    interfaceModuleDict.TryAdd(interfaceType, type);
                                    Utility.Assembly.InvokeMethod(modules[i], LifecycleMethodsConstant.OnInitialization);
                                    moduleCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                Utility.Debug.LogError(e);
                            }
                        }
                        else
                            moduleInitExceptionList.Add(new ArgumentException($"Module : {type} is already exist!"));
                    }
                }
            }
            ActiveModule();
        }
        internal static void InitAppDomainModule()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assemblies = assemblies;
            InitDicts();
            var ModuleManagerType = typeof(IModuleManager);
            var assemblyLength = assemblies.Length;
            for (int h = 0; h < assemblyLength; h++)
            {
                var modules = Utility.Assembly.GetInstancesByAttribute<ModuleAttribute, Module>(assemblies[h]);
                for (int i = 0; i < modules.Length; i++)
                {
                    var type = modules[i].GetType();
                    var module = modules[i];
                    if (ModuleManagerType.IsAssignableFrom(type))
                    {
                        if (!moduleDict.ContainsKey(type))
                        {
                            try
                            {
                                var interfaces = type.GetInterfaces();
                                Type interfaceType = null;
                                foreach (var inter in interfaces)
                                {
                                    if (inter.Name.Contains(type.Name))
                                    {
                                        interfaceType = inter;
                                        break;
                                    }
                                }
                                if (interfaceType != null)
                                {
                                    moduleDict.TryAdd(type, module);
                                    interfaceModuleDict.TryAdd(interfaceType, type);
                                    Utility.Assembly.InvokeMethod(modules[i], LifecycleMethodsConstant.OnInitialization);
                                    moduleCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                Utility.Debug.LogError(e);
                            }
                        }
                        else
                            moduleInitExceptionList.Add(new ArgumentException($"Module : {type} is already exist!"));
                    }
                }
            }
            ActiveModule();
        }
        static void ActiveModule()
        {
            foreach (var module in moduleDict.Values)
            {
                try
                {
                    Utility.Assembly.InvokeMethod((module as Module), LifecycleMethodsConstant.OnActive);
                }
                catch (Exception e)
                {
                    moduleInitExceptionList.Add(e);
                }
            }
            PrepareModule();
        }
        static void PrepareModule()
        {
            foreach (var module in moduleDict.Values)
            {
                try
                {
                    Utility.Assembly.InvokeMethod(module, LifecycleMethodsConstant.OnPreparatory);
                    if (PrintModulePreparatory)
                        Utility.Debug.LogInfo($"Module :{module} is OnPreparatory");
                }
                catch (Exception e)
                {
                    moduleInitExceptionList.Add(e);
                }
            }
            AddRefreshListen();
        }
        static void AddRefreshListen()
        {
            foreach (var module in moduleDict.Values)
            {
                try
                {
                    TickRefreshAttribute.GetRefreshAction(module, true, out var tickAction);
                    if (tickAction != null)
                    {
                        tickRefreshDict.Add(module, tickAction);
                        TickRefreshHandler += tickAction;
                    }
                }
                catch (Exception e)
                {
                    moduleInitExceptionList.Add(e);
                }

            }
            if (moduleInitExceptionList.Count > 0)
            {
                var arr = moduleInitExceptionList.ToArray();
                moduleInitExceptionList.Clear();
                throw new AggregateException(arr);
            }
        }
        static void OnDeactive()
        {
            foreach (var module in moduleDict?.Values)
            {
                try
                {
                    Utility.Assembly.InvokeMethod(module, LifecycleMethodsConstant.OnDeactive);
                }
                catch (Exception e)
                {
                    moduleTerminateExceptionList.Add(e);
                }
            }
            OnTermination();
        }
        static void OnTermination()
        {
            foreach (var module in moduleDict?.Values)
            {
                try
                {
                    Utility.Assembly.InvokeMethod(module, LifecycleMethodsConstant.OnTermination);
                }
                catch (Exception e)
                {
                    moduleTerminateExceptionList.Add(e);
                }
            }
            GameManager.tickRefreshHandler = null;
            tickRefreshDict.Clear();

            if (moduleTerminateExceptionList.Count > 0)
            {
                var arr = moduleTerminateExceptionList.ToArray();
                moduleInitExceptionList.Clear();
                throw new AggregateException(arr);
            }
        }
        static void InitDicts()
        {
            if (moduleDict == null)
            {
                moduleDict = new ConcurrentDictionary<Type, Module>();
                interfaceModuleDict = new Dictionary<Type, Type>();

                moduleInitExceptionList = new List<Exception>();
                moduleTerminateExceptionList = new List<Exception>();

                tickRefreshDict = new Dictionary<object, Action>();
            }
        }
        #endregion
    }
}
