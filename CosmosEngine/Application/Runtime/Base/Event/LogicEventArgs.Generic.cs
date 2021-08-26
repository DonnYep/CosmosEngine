using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Cosmos
{
    /// <summary>
    /// 泛型数据传输容器
    /// 封闭继承
    /// </summary>
    public sealed class LogicEventArgs<T> : LogicEventArgs
    {
        new public T Data { get { return (T)base.Data; } set { base.Data = value; } }
        public LogicEventArgs() { }
        public LogicEventArgs(T data)
        {
            Data = data;
        }
        public override void Release()
        {
            Data = default(T);
        }
    }
}