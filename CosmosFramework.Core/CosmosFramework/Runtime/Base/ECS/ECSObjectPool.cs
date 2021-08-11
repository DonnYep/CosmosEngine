using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.ECS
{
    public class ECSObjectPool : Object
    {
        /// <summary>
        /// type===componentQueue;
        /// </summary>
        readonly Dictionary<Type, ComponentQueue> typeCompQueueDict = new Dictionary<Type, ComponentQueue>();
        static ECSObjectPool instance;
        public static ECSObjectPool Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ECSObjectPool();
                }
                return instance;
            }
        }
        public T Spawn<T>() where T : Object
        {
            T t = (T)Spawn(typeof(T));
            return t;
        }
        public Object Spawn(Type type)
        {
            Object obj;
            if (!this.typeCompQueueDict.TryGetValue(type, out ComponentQueue queue))
            {
                obj = (Object)Activator.CreateInstance(type);
            }
            else if (queue.Count == 0)
            {
                obj = (Object)Activator.CreateInstance(type);
            }
            else
            {
                obj = queue.Dequeue();
            }

            return obj;
        }
        public void Despawn(Object obj)
        {
            var type = obj.GetType();
            if(!typeCompQueueDict.TryGetValue(type,out var queue))
            {
                queue = new ComponentQueue(type.Name);
                typeCompQueueDict.Add(type, queue);
            }
            queue.Enqueue(obj);
        }
        public void Clear()
        {
            foreach (var tc in typeCompQueueDict)
            {
                tc.Value.Dispose();
            }
            typeCompQueueDict.Clear();
        }
        public override void Dispose()
        {
            foreach (var tc in typeCompQueueDict)
            {
                tc.Value.Dispose();
            }
            typeCompQueueDict.Clear();
            instance = null;
        }
    }
}
