using System;
using System.Collections.Generic;
using System.Text;
using Cosmos;
namespace Cosmos.ECS
{
    public class ECSPool<T>where T:class,new ()
    {
        readonly Pool<T> pool;
        public ECSPool()
        {
            pool = new Pool<T>(() => new T());
        }
        public T Spawn()
        {
            return pool.Spawn();
        }
        public void Despawn(T t)
        {
            pool.Despawn(t);
        }
        public void Clear()
        {
            pool.Clear();
        }
    }
}
