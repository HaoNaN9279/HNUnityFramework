using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability
{
    /// <summary>
    /// 对象池管理器（纯 C#）
    /// 管理 ObjectPoolBase / ObjectPool&lt;T&gt; 的注册、查找、Tick 和生命周期。
    /// </summary>
    public sealed class ObjectPoolManager : ITickable
    {
        /// <summary>
        /// 创建一个 Object 对象池（无初始数量）
        /// </summary>
        public T CreateObjectPool<T>(string name) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（无初始数量，指定 Tick 频率）
        /// </summary>
        public T CreateObjectPool<T>(string name, int tickFrequency) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（无初始数量，指定上下限）
        /// </summary>
        public T CreateObjectPool<T>(string name, int tickFrequency, int maxCount, int minCount) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（无初始数量，指定上下限和极限值）
        /// </summary>
        public T CreateObjectPool<T>(string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（指定初始数量）
        /// </summary>
        public T CreateObjectPool<T>(string name, int initialCount, int tickFrequency) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（指定初始数量和上下限）
        /// </summary>
        public T CreateObjectPool<T>(string name, int initialCount, int tickFrequency, int maxCount, int minCount) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个 Object 对象池（指定初始数量、上下限和极限值）
        /// </summary>
        public T CreateObjectPool<T>(string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPoolBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定初始数量和 Tick 频率）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int tickFrequency) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定 Tick 频率和上下限）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int tickFrequency, int maxCount, int minCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定 Tick 频率、上下限和极限值）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定初始数量和 Tick 频率）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定初始数量、Tick 频率和上下限）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency, int maxCount, int minCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个泛型 Object 对象池（指定初始数量、Tick 频率、上下限和极限值）
        /// </summary>
        public T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(name, out _))
            {
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{name}'.");
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 移除一个对象池
        /// </summary>
        public void RemoveObjectPool(string name)
        {
            Remove(name);
        }

        /// <summary>
        /// 获取一个对象池
        /// </summary>
        public PoolBase GetObjectPool(string name)
        {
            return Get(name);
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick()
        {
            foreach (var objectPool in m_objectPools.Values)
            {
                objectPool.Tick();
            }
        }

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public void LateTick()
        {
            foreach (var objectPool in m_objectPools.Values)
            {
                objectPool.LateTick();
            }
        }

        /// <summary>
        /// 对象池数量
        /// </summary>
        public int PoolCount => m_objectPools.Count;

        /// <summary>
        /// 对象池列表（只读）
        /// </summary>
        public IReadOnlyDictionary<string, PoolBase> ObjectPools => m_objectPools;

        private void Add(string name, PoolBase pool)
        {
            if (m_objectPools.ContainsKey(name))
            {
                throw new InvalidOperationException($"ObjectPoolManager: pool '{name}' already exists.");
            }

            m_objectPools.Add(name, pool);
        }

        private void Remove(string name)
        {
            if (!m_objectPools.TryGetValue(name, out PoolBase pool))
            {
                throw new InvalidOperationException($"ObjectPoolManager: pool '{name}' does not exist.");
            }

            ReferencePool.Release(pool);
            m_objectPools.Remove(name);
        }

        private PoolBase Get(string name)
        {
            if (!m_objectPools.TryGetValue(name, out PoolBase pool))
            {
                throw new InvalidOperationException($"ObjectPoolManager: pool '{name}' does not exist.");
            }

            return pool;
        }

        private bool TryGet(string name, out PoolBase pool)
        {
            return m_objectPools.TryGetValue(name, out pool);
        }

        private void ClearAll()
        {
            foreach (var objectPool in m_objectPools.Values)
            {
                objectPool.Clear();
            }

            m_objectPools.Clear();
        }

        private readonly Dictionary<string, PoolBase> m_objectPools = new Dictionary<string, PoolBase>();
    }
}
