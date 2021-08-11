using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Cosmos.ECS
{
    [Flags]
    public enum EntityStatus : byte
    {
        None = 0,
        IsFromPool = 1,
        IsRegister = 1 << 1,
        IsComponent = 1 << 2,
        IsCreate = 1 << 3,
    }
    public partial class Entity : Object
    {
        private static readonly ECSPool<HashSet<Entity>> hashSetPool = new ECSPool<HashSet<Entity>>();

        private static readonly ECSPool<Dictionary<Type, Entity>> dictPool = new ECSPool<Dictionary<Type, Entity>>();

        private static readonly ECSPool<Dictionary<long, Entity>> childrenPool = new ECSPool<Dictionary<long, Entity>>();

        [IgnoreDataMember]
        public long InstanceId
        {
            get;
            set;
        }

        protected Entity()
        {
        }

        [IgnoreDataMember]
        private EntityStatus status = EntityStatus.None;

        [IgnoreDataMember]
        public bool IsFromPool
        {
            get => (this.status & EntityStatus.IsFromPool) == EntityStatus.IsFromPool;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsFromPool;
                }
                else
                {
                    this.status &= ~EntityStatus.IsFromPool;
                }
            }
        }

        [IgnoreDataMember]
        public bool IsRegister
        {
            get => (this.status & EntityStatus.IsRegister) == EntityStatus.IsRegister;
            set
            {
                if (this.IsRegister == value)
                {
                    return;
                }

                if (value)
                {
                    this.status |= EntityStatus.IsRegister;
                }
                else
                {
                    this.status &= ~EntityStatus.IsRegister;
                }

                //EventSystem.Instance.RegisterSystem(this, value);
            }
        }

        [IgnoreDataMember]
        private bool IsComponent
        {
            get => (this.status & EntityStatus.IsComponent) == EntityStatus.IsComponent;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsComponent;
                }
                else
                {
                    this.status &= ~EntityStatus.IsComponent;
                }
            }
        }

        [IgnoreDataMember]
        public bool IsCreate
        {
            get => (this.status & EntityStatus.IsCreate) == EntityStatus.IsCreate;
            set
            {
                if (value)
                {
                    this.status |= EntityStatus.IsCreate;
                }
                else
                {
                    this.status &= ~EntityStatus.IsCreate;
                }
            }
        }

        [IgnoreDataMember]
        public bool IsDisposed { get { return this.InstanceId == 0; } }

        [IgnoreDataMember]
        protected Entity parent;

        [IgnoreDataMember]
        public Entity Parent
        {
            get => this.parent;
            set
            {
                if (value == null)
                {
                    throw new Exception($"cant set parent null: {this.GetType().Name}");
                }

                if (this.parent != null) // 之前有parent
                {
                    // parent相同，不设置
                    if (this.parent.InstanceId == value.InstanceId)
                    {
                        Utility.Debug.LogError($"重复设置了Parent: {this.GetType().Name} parent: {this.parent.GetType().Name}");
                        return;
                    }

                    this.parent.RemoveChild(this);

                    this.parent = value;
                    this.parent.AddChild(this);

                    this.Domain = this.parent.domain;
                }
                else
                {
                    this.parent = value;
                    this.parent.AddChild(this);

                    this.IsComponent = false;
                    this.Domain = this.parent.domain;
                }
            }
        }

        [IgnoreDataMember]
        // 该方法只能在AddComponent中调用，其他人不允许调用
        private Entity ComponentParent
        {
            set
            {
                if (this.parent != null)
                {
                    throw new Exception($"Component parent is not null: {this.GetType().Name}");
                }

                this.parent = value;

                this.IsComponent = true;
                this.Domain = this.parent.domain;
            }
        }

        public long Id { get; set; }

        [IgnoreDataMember]
        protected Entity domain;

        [IgnoreDataMember]
        public Entity Domain
        {
            get => this.domain;
            set
            {
                if (value == null)
                {
                    return;
                }

                Entity preDomain = this.domain;
                this.domain = value;

                if (preDomain == null)
                {
                    this.InstanceId = ECSIdGenerater.Instance.GenerateInstanceId();

                    // 反序列化出来的需要设置父子关系
                    if (!this.IsCreate)
                    {
                        if (this.componentsHashset != null)
                        {
                            foreach (Entity component in this.componentsHashset)
                            {
                                component.IsComponent = true;
                                this.ComponentDict.Add(component.GetType(), component);
                                component.parent = this;
                            }
                        }

                        if (this.childrenHashset != null)
                        {
                            foreach (Entity child in this.childrenHashset)
                            {
                                child.IsComponent = false;
                                this.ChildrenDict.Add(child.Id, child);
                                child.parent = this;
                            }
                        }
                    }
                }

                // 是否注册跟parent一致
                if (this.parent != null)
                {
                    this.IsRegister = this.Parent.IsRegister;
                }

                // 递归设置孩子的Domain
                if (this.childrenDict != null)
                {
                    foreach (Entity entity in this.childrenDict.Values)
                    {
                        entity.Domain = this.domain;
                    }
                }

                if (this.componentDict != null)
                {
                    foreach (Entity component in this.componentDict.Values)
                    {
                        component.Domain = this.domain;
                    }
                }

                if (preDomain == null && !this.IsCreate)
                {
                    //EventSystem.Instance.Deserialize(this);
                }
            }
        }

        [IgnoreDataMember]
        private HashSet<Entity> childrenHashset;

        [IgnoreDataMember]
        private Dictionary<long, Entity> childrenDict;

        [IgnoreDataMember]
        public Dictionary<long, Entity> ChildrenDict
        { get { return this.childrenDict ?? (this.childrenDict = childrenPool.Spawn()); } }

        [IgnoreDataMember]
        private HashSet<Entity> componentsHashset;

        [IgnoreDataMember]
        private Dictionary<Type, Entity> componentDict;

        [IgnoreDataMember]
        public Dictionary<Type, Entity> ComponentDict
        { get { return this.componentDict ?? (this.componentDict = dictPool.Spawn()); } }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            // EventSystem.Instance.Remove(this.InstanceId);
            this.InstanceId = 0;

            // 清理Component
            if (this.componentDict != null)
            {
                foreach (KeyValuePair<Type, Entity> kv in this.componentDict)
                {
                    kv.Value.Dispose();
                }

                this.componentDict.Clear();
                dictPool.Despawn(this.componentDict);
                this.componentDict = null;

                // 从池中创建的才需要回到池中,从db中不需要回收
                if (this.componentsHashset != null)
                {
                    this.componentsHashset.Clear();

                    if (this.IsFromPool)
                    {
                        hashSetPool.Despawn(this.componentsHashset);
                        this.componentsHashset = null;
                    }
                }
            }

            // 清理Children
            if (this.childrenDict != null)
            {
                foreach (Entity child in this.childrenDict.Values)
                {
                    child.Dispose();
                }

                this.childrenDict.Clear();
                childrenPool.Despawn(this.childrenDict);
                this.childrenDict = null;

                if (this.childrenHashset != null)
                {
                    this.childrenHashset.Clear();
                    // 从池中创建的才需要回到池中,从db中不需要回收
                    if (this.IsFromPool)
                    {
                        hashSetPool.Despawn(this.childrenHashset);
                        this.childrenHashset = null;
                    }
                }
            }

            // 触发Destroy事件
            //EventSystem.Instance.Destroy(this);

            this.domain = null;

            if (this.parent != null && !this.parent.IsDisposed)
            {
                if (this.IsComponent)
                {
                    this.parent.RemoveComponent(this);
                }
                else
                {
                    this.parent.RemoveChild(this);
                }
            }

            this.parent = null;

            if (this.IsFromPool)
            {
                ECSObjectPool.Instance.Despawn(this);
            }
            else
            {
                base.Dispose();
            }

            status = EntityStatus.None;
        }
        public K GetChild<K>(long id) where K : Entity
        {
            if (this.childrenDict == null)
            {
                return null;
            }
            this.childrenDict.TryGetValue(id, out Entity child);
            return child as K;
        }
        public Entity AddComponent(Entity component)
        {
            Type type = component.GetType();
            if (this.componentDict != null && this.componentDict.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            component.ComponentParent = this;

            this.AddToComponent(type, component);

            return component;
        }
        public Entity AddComponent(Type type)
        {
            if (this.componentDict != null && this.componentDict.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            Entity component = CreateWithComponentParent(type);

            this.AddToComponent(type, component);

            return component;
        }
        public K AddComponent<K>() where K : Entity, new()
        {
            Type type = typeof(K);
            if (this.componentDict != null && this.componentDict.ContainsKey(type))
            {
                throw new Exception($"entity already has component: {type.FullName}");
            }

            K component = CreateWithComponentParent<K>();

            this.AddToComponent(type, component);

            return component;
        }
        public void RemoveComponent<K>() where K : Entity
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.componentDict == null)
            {
                return;
            }

            Type type = typeof(K);
            Entity c = this.GetComponent(type);
            if (c == null)
            {
                return;
            }

            this.RemoveFromComponent(type, c);
            c.Dispose();
        }
        public void RemoveComponent(Entity component)
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (this.componentDict == null)
            {
                return;
            }

            Type type = component.GetType();
            Entity c = this.GetComponent(component.GetType());
            if (c == null)
            {
                return;
            }

            if (c.InstanceId != component.InstanceId)
            {
                return;
            }

            this.RemoveFromComponent(type, c);
            c.Dispose();
        }
        public void RemoveComponent(Type type)
        {
            if (this.IsDisposed)
            {
                return;
            }

            Entity c = this.GetComponent(type);
            if (c == null)
            {
                return;
            }

            RemoveFromComponent(type, c);
            c.Dispose();
        }
        public virtual K GetComponent<K>() where K : Entity
        {
            if (this.componentDict == null)
            {
                return null;
            }

            Entity component;
            if (!this.componentDict.TryGetValue(typeof(K), out component))
            {
                return default;
            }

            return (K)component;
        }
        public virtual Entity GetComponent(Type type)
        {
            if (this.componentDict == null)
            {
                return null;
            }

            Entity component;
            if (!this.componentDict.TryGetValue(type, out component))
            {
                return null;
            }

            return component;
        }
        private void RemoveFromComponentsDB(Entity component)
        {
            if (this.componentsHashset == null)
            {
                return;
            }
            this.componentsHashset.Remove(component);
            if (this.componentsHashset.Count == 0 && this.IsFromPool)
            {
                hashSetPool.Despawn(componentsHashset);
                this.componentsHashset = null;
            }
        }
        private void AddToComponent(Type type, Entity component)
        {
            if (this.componentDict == null)
            {
                this.componentDict = dictPool.Spawn();
            }

            this.componentDict.Add(type, component);

            //if (component is ISerializeToEntity)
            //{
            //    this.AddToComponentsDB(component);
            //}
        }
        private void RemoveFromComponent(Type type, Entity component)
        {
            if (this.componentDict == null)
            {
                return;
            }

            this.componentDict.Remove(type);

            if (this.componentDict.Count == 0 && this.IsFromPool)
            {
                dictPool.Despawn(this.componentDict);
                this.componentDict = null;
            }

            this.RemoveFromComponentsDB(component);
        }
        private void AddChild(Entity entity)
        {
            this.ChildrenDict.Add(entity.Id, entity);
            if (this.childrenHashset == null)
            {
                this.childrenHashset = hashSetPool.Spawn();
            }
            this.childrenHashset.Add(entity);
        }
        private void RemoveChild(Entity entity)
        {
            if (this.childrenDict == null)
            {
                return;
            }

            this.childrenDict.Remove(entity.Id);

            if (this.childrenDict.Count == 0)
            {
                childrenPool.Despawn(this.childrenDict);
                this.childrenDict = null;
            }

            if (this.childrenHashset == null)
            {
                return;
            }

            this.childrenHashset.Remove(entity);

            if (this.childrenHashset.Count == 0)
            {
                if (this.IsFromPool)
                {
                    hashSetPool.Despawn(this.childrenHashset);
                    this.childrenHashset = null;
                }
            }
        }
    }
}
