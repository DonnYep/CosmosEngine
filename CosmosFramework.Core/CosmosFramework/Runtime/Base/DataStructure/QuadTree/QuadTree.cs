using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace Cosmos.QuadTree
{
    /// <summary>
    /// 四叉树；
    /// </summary>
    /// <typeparam name="T">四叉树自定义的数据模型</typeparam>
    public class QuadTree<T>
    {
        readonly static QuadTreePool<QuadTree<T>> nodePool;
        readonly static QuadTreePool<List<T>> listPool;
        readonly static QuadTreePool<HashSet<T>> hashSetPool;
        static QuadTree()
        {
            nodePool = new QuadTreePool<QuadTree<T>>(() => new QuadTree<T>(), node => { node.Release(); });
            listPool = new QuadTreePool<List<T>>(() => new List<T>(), lst => { lst.Clear(); });
            hashSetPool = new QuadTreePool<HashSet<T>>(() => new HashSet<T>(), hs => { hs.Clear(); });
        }
        public static QuadTree<T> Create(float x, float y, float width, float height, IObjecRectangletBound<T> quadTreebound, int objectCapacity = 10, int maxDepth = 5)
        {
            return Create(x, y, width, height, null, quadTreebound, objectCapacity, maxDepth, 0);
        }
        public static void Release(QuadTree<T> node)
        {
            nodePool.Despawn(node);
        }
        /// <summary>
        /// 释放当前树的静态数据；
        /// </summary>
        public static void Dispose()
        {
            nodePool.Clear();
            listPool.Clear();
            hashSetPool.Clear();
        }
        static QuadTree<T> Create(float x, float y, float width, float height, QuadTree<T> parent, IObjecRectangletBound<T> quadTreebound, int objectCapacity = 10, int maxDepth = 5, int currentDepth = 0)
        {
            var node = nodePool.Spawn();
            node.SetNode(x, y, width, height, quadTreebound, objectCapacity, maxDepth, currentDepth);
            return node;
        }

        /// <summary>
        ///当前所在的区块；
        /// </summary>
        public QuadRectangle Area { get; private set; }
        /// <summary>
        /// 当前深度；
        /// </summary>
        public int CurrentDepth { get; private set; }
        public int MaxDepth { get; private set; }
        /// <summary>
        /// 当前Rect的对象数量；
        /// </summary>
        public int ObjectCapacity { get; private set; }
        /// <summary>
        /// 当前树节点的物体对象集合；
        /// </summary>
        HashSet<T> objectSet;
        /// <summary>
        /// 四叉树中对象的有效边界获取接口；
        /// </summary>
        IObjecRectangletBound<T> objectRectangleBound;
        /// <summary>
        /// 是否存在子节点；
        /// </summary>
        bool hasChildren;
        /// <summary>
        /// 父节点；
        /// </summary>
        QuadTree<T> parent;
        /// <summary>
        /// TopRight Quadrant1
        /// </summary>
        QuadTree<T> treeTR;
        /// <summary>
        /// TopLeft Quadrant2
        /// </summary>
        QuadTree<T> treeTL;
        /// <summary>
        /// BottomLeft Quadrant3
        /// </summary>
        QuadTree<T> treeBL;
        /// <summary>
        /// BottomRight Quadrant4
        /// </summary>
        QuadTree<T> treeBR;

        public void CheckObjectRect()
        {
            if (hasChildren)
            {
                treeTR.CheckObjectRect();
                treeTL.CheckObjectRect();
                treeBL.CheckObjectRect();
                treeBR.CheckObjectRect();

                //var trCount = treeTR.Count();
                //var tlCount = treeTL.Count();
                //var brCount = treeBR.Count();
                //var blCount = treeBL.Count();

                //var amount = trCount + tlCount + brCount + blCount;
                //if (amount ==0)
                //    CombineQuads();
            }

            else
            {
                var list = listPool.Spawn();
                foreach (var obj in objectSet)
                {
                    //遍历，如果不符合区域包含，则添加到移除列表中；
                    if (!IsObjectOverlapping(obj))
                    {
                        list.Add(obj);
                    }
                }
                var length = list.Count;
                for (int i = 0; i < length; i++)
                {
                    var obj = list[i];
                    //从当前树中移除；
                    objectSet.Remove(obj);
                    //若父类不为空，并且符合包含规则，则添加入父类的区块中；
                    if (parent != null)
                        parent.InsertToParent(obj);
                }
                listPool.Despawn(list);
            }
        }
        public bool Insert(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (!IsObjectOverlapping(obj)) return false;
            if (hasChildren)
            {
                if (treeTL.Insert(obj)) return true;
                if (treeTR.Insert(obj)) return true;
                if (treeBL.Insert(obj)) return true;
                if (treeBR.Insert(obj)) return true;
            }
            else
            {
                var result = objectSet.Add(obj);
                if (!result)
                    return !result;
                if (objectSet.Count > ObjectCapacity)
                {
                    Quarter();
                }
            }
            return true;
        }
        /// <summary>
        ///插入一个对象集合； 
        /// </summary>
        public void InsertRange(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                Insert(obj);
            }
        }
        public bool Remove(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (!IsObjectOverlapping(obj)) return false;
            if (hasChildren)
            {
                if (treeTL.Remove(obj)) return true;
                if (treeTR.Remove(obj)) return true;
                if (treeBL.Remove(obj)) return true;
                if (treeBR.Remove(obj)) return true;
                var tlCount = treeTL.Count();
                var trCount = treeTR.Count();
                var blCount = treeBL.Count();
                var brCount = treeBR.Count();
                var result = tlCount & trCount & blCount & brCount;
                if (result == 0)
                    CombineQuads();
            }
            else
            {
                objectSet.Remove(obj);
            }
            return true;
        }
        public void RemoveRange(IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                Remove(obj);
            }
        }
        /// <summary>
        ///获取当前树中的最深深度； 
        /// </summary>
        public int CurrentQuadTreeDepth()
        {
            if (hasChildren)
            {
                var trDepth = treeTL.CurrentQuadTreeDepth();
                var tlDepth = treeTL.CurrentQuadTreeDepth();
                var blDepth = treeBL.CurrentQuadTreeDepth();
                var brDepth = treeBR.CurrentQuadTreeDepth();
                var depthArr = new int[4] { trDepth, tlDepth, blDepth, brDepth };
                var depth = trDepth;
                var length = depthArr.Length;
                for (int i = 0; i < length; i++)
                {
                    if (depth < depthArr[i])
                    {
                        depth = depthArr[i];
                    }
                }
                return depth;
            }
            else
                return CurrentDepth;
        }
        /// <summary>
        ///获取当前树中元素的数量；  
        /// </summary>
        public int Count()
        {
            int count = 0;
            if (hasChildren)
            {
                count += treeTR.Count();
                count += treeTL.Count();
                count += treeBR.Count();
                count += treeBL.Count();
            }
            else
            {
                count = objectSet.Count;
            }
            return count;
        }
        /// <summary>
        /// 获取区块中的所有对象；
        /// </summary>
        /// <param name="rect">rect区块</param>
        /// <returns>区块中的所有对象</returns>
        public T[] GetAreaObjects(QuadRectangle rect)
        {
            if (hasChildren)
            {
                if (treeTL.IsRectOverlapping(rect)) return treeTL.GetAreaObjects(rect);
                if (treeTR.IsRectOverlapping(rect)) return treeTR.GetAreaObjects(rect);
                if (treeBL.IsRectOverlapping(rect)) return treeBL.GetAreaObjects(rect);
                if (treeBR.IsRectOverlapping(rect)) return treeBR.GetAreaObjects(rect);
            }
            else
            {
                if (IsRectOverlapping(rect))
                {
                    return objectSet.ToArray();
                }
            }
            return new T[0];
        }
        /// <summary>
        /// 获取对象所在区块中所有相同深度的对象；
        /// </summary>
        /// <param name="go">树中的对象</param>
        /// <returns>相同级别的对象</returns>
        public T[] GetAreaObjects(T go)
        {
            return GetAreaObjects(new QuadRectangle(objectRectangleBound.GetCenterX(go), objectRectangleBound.GetCenterY(go), objectRectangleBound.GetWidth(go), objectRectangleBound.GetHeight(go)));
        }
        public T[] GetAllObjects()
        {
            if (hasChildren)
            {
                List<T> foundObjects = listPool.Spawn();
                foundObjects.AddRange(treeTR.GetAllObjects());
                foundObjects.AddRange(treeTL.GetAllObjects());
                foundObjects.AddRange(treeBR.GetAllObjects());
                foundObjects.AddRange(treeBL.GetAllObjects());
                var arr = foundObjects.ToArray();
                listPool.Despawn(foundObjects);
                return arr;
            }
            else
            {
                return objectSet.ToArray();
            }
        }
        public QuadRectangle GetObjectGrid(T go)
        {
            var rect = new QuadRectangle(objectRectangleBound.GetCenterX(go), objectRectangleBound.GetCenterY(go), objectRectangleBound.GetWidth(go), objectRectangleBound.GetHeight(go));
            return GetRectGrid(rect);
        }
        public QuadRectangle GetRectGrid(QuadRectangle rect)
        {
            if (!IsRectOverlapping(rect))
                return QuadRectangle.Zero;
            if (hasChildren)
            {
                var tlRect = treeTL.GetRectGrid(rect);
                if (tlRect != QuadRectangle.Zero) return tlRect;
                var trRect = treeTR.GetRectGrid(rect);
                if (trRect != QuadRectangle.Zero) return trRect;
                var blRect = treeBL.GetRectGrid(rect);
                if (blRect != QuadRectangle.Zero) return blRect;
                var brRect = treeBR.GetRectGrid(rect);
                if (brRect != QuadRectangle.Zero) return brRect;
            }
            else
            {
                return Area;
            }
            return QuadRectangle.Zero;
        }
        public QuadRectangle[] GetGrid()
        {
            List<QuadRectangle> grid = new List<QuadRectangle> { Area };
            if (hasChildren)
            {
                grid.AddRange(treeTR.GetGrid());
                grid.AddRange(treeTL.GetGrid());
                grid.AddRange(treeBR.GetGrid());
                grid.AddRange(treeBL.GetGrid());
            }
            return grid.ToArray();
        }
        private QuadTree() { }
        void InsertToParent(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (!IsObjectOverlapping(obj))
            {
                if (parent != null)
                    parent.InsertToParent(obj);
            }
            else
                Insert(obj);
        }
        void SetNode(float x, float y, float width, float height, IObjecRectangletBound<T> quadTreebound, int objectCapacity = 10, int maxDepth = 5, int currentDepth = 0)
        {
            Area = new QuadRectangle(x, y, width, height);
            objectSet = hashSetPool.Spawn();
            this.objectRectangleBound = quadTreebound;
            this.CurrentDepth = currentDepth;
            this.MaxDepth = maxDepth;
            this.ObjectCapacity = objectCapacity;
            hasChildren = false;
        }
        void Quarter()
        {
            if (CurrentDepth > MaxDepth) return;
            int nextDepth = CurrentDepth + 1;
            hasChildren = true;
            treeTR = Create(Area.X + Area.HalfWidth * 0.5f, Area.Y + Area.HalfHeight * 0.5f, Area.HalfWidth, Area.HalfHeight, this, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeTL = Create(Area.X - Area.HalfWidth * 0.5f, Area.Y + Area.HalfHeight * 0.5f, Area.HalfWidth, Area.HalfHeight, this, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeBL = Create(Area.X - Area.HalfWidth * 0.5f, Area.Y - Area.HalfHeight * 0.5f, Area.HalfWidth, Area.HalfHeight, this, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            treeBR = Create(Area.X + Area.HalfWidth * 0.5f, Area.Y - Area.HalfHeight * 0.5f, Area.HalfWidth, Area.HalfHeight, this, objectRectangleBound, ObjectCapacity, MaxDepth, nextDepth);
            foreach (var obj in objectSet)
            {
                Insert(obj);
            }
            objectSet.Clear();
        }
        void CombineQuads()
        {
            hasChildren = false;
            objectSet = hashSetPool.Spawn();
            Release(treeTL);
            Release(treeTR);
            Release(treeBL);
            Release(treeBR);
        }
        bool IsRectOverlapping(QuadRectangle rect)
        {
            if (rect.Right < Area.Left || rect.Left > Area.Right) return false;
            if (rect.Top < Area.Bottom || rect.Bottom > Area.Top) return false;
            return true;
        }
        /// <summary>
        /// 对象是否存在于当前rect中；
        /// </summary>
        bool IsObjectOverlapping(T go)
        {
            var x = objectRectangleBound.GetCenterX(go);
            var y = objectRectangleBound.GetCenterY(go);
            var width = objectRectangleBound.GetWidth(go);
            var height = objectRectangleBound.GetHeight(go);
            return IsRectOverlapping(new QuadRectangle(x, y, width, height));
        }
        void Release()
        {
            if (hasChildren)
            {
                nodePool.Despawn(treeTR);
                nodePool.Despawn(treeTL);
                nodePool.Despawn(treeBR);
                nodePool.Despawn(treeBL);
                treeTR = null;
                treeTL = null;
                treeBR = null;
                treeBL = null;
            }
            hashSetPool.Despawn(objectSet);
            objectSet = null;
            hasChildren = false;
            parent = null;
        }
    }
}