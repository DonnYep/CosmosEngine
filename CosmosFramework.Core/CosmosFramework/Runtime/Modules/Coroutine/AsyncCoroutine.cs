using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cosmos
{

    /// <summary>
    /// 异步单线程协程测案例
    /// </summary>
/*
static void AsyncCoroutineTest()
{
    Task.Run(AsyncCoroutine.Instance.Start);
    Console.WriteLine(DateTime.Now.Ticks / 10000);
    Console.WriteLine(Utility.Time.MillisecondNow());
    AsyncCoroutine.Instance.WaitTimeAsyncCallback(1001, () =>
    {
        Console.WriteLine("Callback01");
        Task.Run(() => { AsyncCoroutine.Instance.WaitTimeAsyncCallback(2300, () => Console.WriteLine("Callback0233")); });
    });
    AsyncCoroutine.Instance.WaitTimeAsyncCallback(1500, () => Console.WriteLine("Callback02"));
    AsyncCoroutine.Instance.WaitTimeAsyncCallback(2000, () => Console.WriteLine("Callback03"));
    AsyncCoroutine.Instance.WaitTimeAsyncCallback(3000, () =>
    {
        Console.WriteLine("Callback04");
        AsyncCoroutine.Instance.WaitTimeAsyncCallback(6000, () => Console.WriteLine("Callback05"));
    });
    Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
    AsyncCoroutine.Instance.WaitTimeAsyncCallback(15001, () =>
    {
        Console.WriteLine("Callback06");
        Console.WriteLine(AsyncCoroutine.Instance.CorouCount);
    });
}
*/

/// <summary>
/// 单线程异步协程，需要由外部主线程开启；
/// 单线程异步减少多线程的开启消耗；
/// 开启协程请调用 Start；
/// 生命周期：Task.Run(Start)->OnPause/OnUnPause->Abort->OnRenewal->Task.Run(Start);
/// </summary>
public class AsyncCoroutine : ConcurrentSingleton<AsyncCoroutine>, IControllable, IRenewable
{
    /// <summary>
    /// 内部的协程结构体
    /// </summary>
    private struct CoroutineStruct
    {
        public CoroutineStruct(long dispatchTime, Action handler)
        {
            DispatchTime = dispatchTime;
            Handler = handler;
        }
        /// <summary>
        /// 触发时间
        /// </summary>
        public long DispatchTime { get; private set; }
        /// <summary>
        /// 委托
        /// </summary>
        public Action Handler { get; private set; }
    }
    public bool IsPause { get; private set; } = false;
    public int CorouCount { get { return corouSet.Count; } }
    List<CoroutineStruct> corouSet = new List<CoroutineStruct>();
    HashSet<int> removeSet = new HashSet<int>();
    bool canRun = true;
    /// <summary>
    /// 开始程协程；
    /// 使用时请使用如下代码：
    /// 服务器使用：使用一个线程或者在主线程中使用While(true)开启；
    /// 客户端使用：在Update或者其他帧刷新的方法中调用；
    /// </summary>
    public void Start()
    {
        while (canRun)
        {
            if (!IsPause)
            {
                OnRefresh();
            }
        }
    }
    /// <summary>
    /// 终止这个线程，并清空已经添加的协程;
    /// </summary>
    public void Abort()
    {
        canRun = false;
        corouSet.Clear();
        removeSet.Clear();
    }
    /// <summary>
    /// 单线程异步委托
    /// </summary>
    /// <param name="waitTime">等待时间，这里是ms</param>
    /// <param name="callback">回调</param>
    public void WaitTimeAsyncCallback(int waitTime, Action callback)
    {
        var dispatchTime = Utility.Time.MillisecondNow() + waitTime;
        CoroutineStruct corou = new CoroutineStruct(dispatchTime, callback);
        corouSet.Add(corou);
    }
    public void OnPause()
    {
        IsPause = true;
    }
    public void OnUnPause()
    {
        IsPause = false;
    }
    /// <summary>
    /// 重置这个单线程异步对象；
    /// </summary>
    public void OnRenewal()
    {
        canRun = true;
    }
     void OnRefresh()
    {
        if (corouSet.Count <= 0)
            return;
        int length = corouSet.Count;
        removeSet.Clear();
        for (int i = 0; i < length; i++)
        {
            if (corouSet[i].DispatchTime <= Utility.Time.MillisecondNow())
            {
                corouSet[i].Handler.Invoke();
                removeSet.Add(i);
            }
        }
        foreach (var c in removeSet)
        {
            corouSet.RemoveAt(c);
        }
    }
}
}
