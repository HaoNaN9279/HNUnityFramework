using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Capability
{
    /// <summary>
    /// 对象池管理器（纯 C#）
    /// 管理 ObjectPoolBase / ObjectPool&lt;T&gt; 的注册、查找、Tick 和生命周期。
    /// </summary>
    public sealed class ObjectPoolManager : ITickable
    {
        /// <summary>
        /// 创建纯 C# 对象池
        /// </summary>
        /// <param name="settings">对象池配置参数</param>
        /// <typeparam name="T">对象池类型</typeparam>
        /// <returns>创建的对象池实例</returns>
        public T CreateObjectPool<T>(PoolSettings settings) where T : ObjectPoolBase, new()
        {
            if (TryGet(settings.Name, out _))
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{settings.Name}'.");

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(settings);
            Add(settings.Name, pool);
            return pool;
        }

        /// <summary>
        /// 创建泛型 C# 对象池
        /// </summary>
        /// <param name="settings">对象池配置参数</param>
        /// <typeparam name="T">对象池类型</typeparam>
        /// <typeparam name="U">池中对象类型</typeparam>
        /// <returns>创建的对象池实例</returns>
        public T CreateObjectPool<T, U>(PoolSettings settings) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (TryGet(settings.Name, out _))
                throw new InvalidOperationException($"ObjectPoolManager already contains pool '{settings.Name}'.");

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(settings);
            Add(settings.Name, pool);
            return pool;
        }

        /// <summary>
        /// 注册已初始化的池实例（用于 GameObjectPool 等由外部创建的池）
        /// </summary>
        /// <param name="name">池名称</param>
        /// <param name="pool">池实例</param>
        public void RegisterPool(string name, PoolBase pool)
        {
            Add(name, pool);
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

        public void ClearAll()
        {
            foreach (var objectPool in m_objectPools.Values)
            {
                objectPool.Clear();
                ReferencePool.Release(objectPool);
            }

            m_objectPools.Clear();
        }

        private readonly Dictionary<string, PoolBase> m_objectPools = new Dictionary<string, PoolBase>();
    }
}
