using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos
{
    public class LogicEventArgs : GameEventArgs
    {
        public object Data { get; set; }
        public LogicEventArgs() { }
        public LogicEventArgs(object data)
        {
            Data = data;
        }
        public override void Release()
        {
            Data = null;
        }
    }
}
