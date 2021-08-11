using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Cosmos.ECS
{
    public abstract class Object : ISupportInitialize, IDisposable
    {
        public virtual void BeginInit(){}
        public virtual void EndInit(){}
        public virtual void Dispose(){}
    }
}
