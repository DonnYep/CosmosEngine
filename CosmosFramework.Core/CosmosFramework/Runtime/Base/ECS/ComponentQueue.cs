using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.ECS
{
    public class ComponentQueue : Object
    {
        readonly Queue<Object> queue = new Queue<Object>();
        public Queue<Object> Queue { get { return queue; } }
        public int Count { get { return queue.Count; } }
        public string TypeName{get;private set;}
        public ComponentQueue(string typeName)
        {
            this.TypeName = typeName;
        }
        public void Enqueue(Object entity)
        {
            this.queue.Enqueue(entity);
        }
        public Object Dequeue()
        {
            return this.queue.Dequeue();
        }
        public Object Peek()
        {
            return this.queue.Peek();
        }
        public override void Dispose()
        {
            while (this.queue.Count > 0)
            {
                Object component = this.queue.Dequeue();
                component.Dispose();
            }
        }
    }
}
