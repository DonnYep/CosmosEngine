using System;
using System.Collections;
using System.Collections.Generic;
namespace Cosmos
{
    public class SkipList<T> : ICollection<T>
        where T : IComparable
    {
        internal SkipListNode<T> topLeft;
        internal SkipListNode<T> bottomLeft;
        internal Random random;
        private int currentLevel;
        /// <summary>
        /// 节点数量；
        /// Node count 
        /// </summary>
        private int nodeCount;
        private int maxLevels = int.MaxValue;
        public SkipList()
        {
            topLeft = new SkipListNode<T>(default(T));
            bottomLeft = topLeft;
            currentLevel = 1;
            nodeCount = 0;
            random = new Random(); //used for adding new values
        }
        /// <summary>
        /// 当前跳表层数；
        /// </summary>
        public int Level { get { return currentLevel; } }
        public int MaxLevels { get { return maxLevels; } set { maxLevels = value; } }
        /// <summary>
        /// 节点数量；
        /// </summary>
        public int Count { get { return nodeCount; } }
        public bool IsReadOnly { get { return false; } }
        public SkipListNode<T> Head { get { return bottomLeft; } }
        public void Add(T value)
        {
            //添加流程：
            //1. 投掷硬币，获取到新加入节点的层级
            //2. 比较节点值，加入最下层的有序链表
            //3. 分配查询层级节点

            int valueLevel = CoinFlipLevel();
            int valueLevelCount = valueLevel - currentLevel;
            while (valueLevelCount > 0)
            {
                //补全TopLeft节点
                var newNode = new SkipListNode<T>(topLeft.Value);
                newNode.Below = topLeft;
                topLeft.Above = newNode;
                topLeft = newNode;

                valueLevelCount--;
                currentLevel++;
            }
            SkipListNode<T> currentNode = topLeft;
            SkipListNode<T> lastNodeAbove = null;
            var remainLevel = currentLevel - 1;//剩余的层数

            while (currentNode != null && remainLevel >= 0)
            {
                if (remainLevel > currentLevel)
                {
                    //进入到当前节点的高度
                    currentNode = currentNode.Below;
                    remainLevel--;
                    continue;
                }
                while (currentNode.Next != null)
                {
                    if (currentNode.Next.Value.CompareTo(value) < 0)
                    {
                        currentNode = currentNode.Next;
                    }
                    else
                    {
                        break;// nextNode节点的值大于等于当前插入的值
                    }
                }
                var newNode = new SkipListNode<T>(value);
                newNode.Next = currentNode.Next;
                newNode.Previous = currentNode;
                newNode.Next.Previous = newNode;
                currentNode.Next = newNode;
                if (lastNodeAbove != null)
                {
                    lastNodeAbove.Below = newNode;
                    newNode.Above = lastNodeAbove;
                }
                lastNodeAbove = newNode;
                currentNode = currentNode.Below;
                remainLevel--;
            }
            nodeCount++;
        }
        public void Clear()
        {
            SkipListNode<T> currentNode = this.Head;
            while (currentNode != null)
            {
                SkipListNode<T> nextNode = currentNode.Next; //save reference to next node
                this.Remove(currentNode);
                currentNode = nextNode;
            }
        }
        public bool Contains(T value)
        {
            return FindNode(value) != null;
        }
        public SkipListNode<T> FindNode(T value)
        {
            var currentNode = topLeft;
            while (currentNode != null)
            {
                if (currentNode.Next != null && currentNode.Value.CompareTo(value) < 0)//node.Value小于value且node.Next不为空
                {
                    currentNode = currentNode.Next;
                }
                else
                {
                    //node.Next为空或node.Value大于等于value
                    var nextNode = currentNode.Next;
                    if (nextNode != null && nextNode.Value.CompareTo(value) == 0)
                    {
                        return nextNode;
                    }
                    else//nextNode为空或者nextNode.Value大于value
                    {
                        currentNode = currentNode.Below;
                    }
                }
            }
            return currentNode;
        }
        public void CopyTo(T[] array)
        {
            CopyTo(array, 0);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            IEnumerator<T> enumerator = this.GetEnumerator();

            for (int i = arrayIndex; i < array.Length; i++)
            {
                if (enumerator.MoveNext())
                {
                    array[i] = enumerator.Current;
                }
                else
                {
                    break;
                }
            }
        }
        public bool Remove(T value)
        {
            var valueNode = FindHighest(value);
            return Remove(valueNode);
        }
        public bool Remove(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return false;
            }
            else
            {
                if (valueNode.Above != null)
                {
                    valueNode = FindHighest(valueNode);
                }
                var currentNodeDown = valueNode;
                while (currentNodeDown != null)
                {
                    var previousNode = currentNodeDown.Previous;
                    var nextNode = currentNodeDown.Next;

                    previousNode.Next = nextNode;
                    nextNode.Previous = previousNode;

                    var belowNode = currentNodeDown.Below;
                    currentNodeDown.Dispose();

                    currentNodeDown = belowNode;
                }
                nodeCount--;
                ClearEmptyLevels();
                return true;
            }
        }
        public SkipListNode<T> Find(T value)
        {
            var foundNode = this.topLeft;
            while (foundNode != null && foundNode.Next != null)
            {
                if ( foundNode.Next.Value.CompareTo(value) < 0)
                {
                    foundNode = foundNode.Next;
                }
                else
                {
                    if ( foundNode.Next.Value.Equals(value))
                    {
                        foundNode = foundNode.Next;
                        break;
                    }
                    else
                    {
                        foundNode = foundNode.Below;
                    }
                }
            }

            return foundNode;
        }
        /// <summary>
        /// 获取最高的节点；
        /// 若不存在，则返回空；
        /// </summary>
        /// <param name="valueNode">查询的目标节点</param>
        /// <returns>最高的节点</returns>
        public SkipListNode<T> FindHighest(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return null;
            }
            else
            {
                while (valueNode.Above != null)
                {
                    valueNode = valueNode.Above;
                }
                return valueNode;
            }
        }
        public virtual SkipListNode<T> FindHighest(T value)
        {
            SkipListNode<T> valueNode = this.Find(value);
            return this.FindHighest(valueNode);
        }
        public SkipListNode<T> FindLowest(T value)
        {
            SkipListNode<T> valueNode = this.Find(value);
            return this.FindLowest(valueNode);
        }
        public SkipListNode<T> FindLowest(SkipListNode<T> valueNode)
        {
            if (valueNode == null)
            {
                return null;
            }
            else
            {
                while (valueNode.Below != null)
                {
                    valueNode = valueNode.Below;
                }
                return valueNode;
            }
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new SkipListEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        /// <summary>
        /// 投掷硬币；
        /// Coin flip;
        /// 1 is heads, 0 is tails;
        /// </summary>
        /// <returns>coin value</returns>
        int CoinFlipLevel()
        {
            int level = 0;
            while (random.Next(0, 1) == 1 && level < currentLevel)//投掷硬币为1时增加一层
            {
                level++;
            }
            return level;
        }
        void ClearEmptyLevels()
        {
            if (currentLevel > 1)
            {
                var currentNode = topLeft;
                while (currentNode != bottomLeft)
                {
                    if (currentNode.Next == null)
                    {
                        var belowNode = currentNode.Below;
                        topLeft = currentNode.Below;

                        currentNode.Next.Dispose();
                        currentNode.Dispose();
                        currentLevel--;
                        currentNode = belowNode;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// 跳表迭代对象，遍历最低一层的跳表；
        /// </summary>
        public class SkipListEnumerator : IEnumerator<T>
        {
            private SkipListNode<T> current;
            private SkipList<T> skipList;

            public SkipListEnumerator(SkipList<T> skipList)
            {
                this.skipList = skipList;
            }
            public T Current { get { return current.Value; } }
            object IEnumerator.Current { get { return this.Current; } }
            public void Dispose()
            {
                current = null;
            }
            public void Reset()
            {
                current = null;
            }
            public bool MoveNext()
            {
                if (current == null)
                {
                    current = this.skipList.Head.Next;    //Head is header node, start after
                }
                else
                {
                    current = current.Next;
                }
                return (current != null);
            }
        }
    }
}
