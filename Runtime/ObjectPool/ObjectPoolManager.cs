using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    /// <summary>
    /// 对象池管理器
    /// 依赖Reference Pool
    /// </summary>
    public sealed class ObjectPoolManager : ITickable
    {
        /// <summary>
        /// 对象池管理器初始化
        /// </summary>
        public static void Initialize(GameObject managerRoot = null)
        {
            if (s_instance == null)
            {
                s_instance = new ObjectPoolManager();
            }

            if (s_managerRoot == null)
            {
                s_managerRoot = managerRoot;
            }

            if (s_managerRoot == null)
            {
                s_managerRoot = GameObject.Find(s_managerRootName);
            }

            if (s_managerRoot == null)
            {
                s_managerRoot = new GameObject(s_managerRootName);
                Object.DontDestroyOnLoad(s_managerRoot);
            }

#if UNITY_EDITOR
            if (s_viewer == null)
            {
                s_viewer = s_managerRoot.AddComponent<ObjectPoolViewer>();
                s_viewer.Initialize(s_instance);
            }
#endif
        }

        /// <summary>
        /// 对象池管理器销毁
        /// </summary>
        public static void Uninitialize()
        {
            s_instance.ClearAll();

            if (s_managerRoot != null)
            {
                Object.Destroy(s_managerRoot);
            }
        }

        /// <summary>
        /// 对象池管理器每帧更新
        /// </summary>
        public static void TickObjectPoolManager()
        {
            s_instance.Tick();
        }

        /// <summary>
        /// 对象池管理器每帧后更新
        /// </summary>
        public static void LateTickObjectPoolManager()
        {
            s_instance.LateTick();
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T>(string name) where T : ObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }

            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T>(string name, int tickFrequency) where T : ObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T>(string name, int tickFrequency, int maxCount, int minCount) where T : ObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T>(string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency, int maxCount, int minCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个Object对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        /// <returns></returns>
        public static T CreateObjectPool<T, U>(string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : ObjectPool<U>, new() where U : PooledObjectBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(name, initialCount, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int tickFrequency) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, tickFrequency);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int tickFrequency, int maxCount, int minCount) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, tickFrequency, maxCount, minCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int initialCount, int tickFrequency) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, initialCount, tickFrequency);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, initialCount, tickFrequency, maxCount, minCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 创建一个GameObject对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        /// <returns></returns>
        public static T CreateGameObjectPool<T>(GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount) where T : GameObjectPoolBase, new()
        {
            if (s_instance.TryGet(name, out PoolBase tempPool))
            {
                Debug.LogError($"ObjectPoolManger already contains {name}.");
                return default;
            }
            
            T pool = ReferencePool.Acquire<T>();
            pool.Initialize(s_managerRoot, prototype, name, initialCount, tickFrequency, maxCount, minCount, maxLimitCount, minLimitCount);
            s_instance.Add(name, pool);
            return pool;
        }

        /// <summary>
        /// 移除一个对象池
        /// </summary>
        /// <param name="name"></param>
        public static void RemoveObjectPool(string name) => s_instance.Remove(name);

        /// <summary>
        /// 获取一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PoolBase GetObjectPool(string name) => s_instance.Get(name);        


        /// <summary>
        /// 添加一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        private void Add(string name, PoolBase pool)
        {
            if (m_objectPools.ContainsKey(name))
            {
                Debug.LogError($"ObjectPoolManager: AddPool: pool {name} is already exist.");
                return;
            }

            m_objectPools.Add(name, pool);
        }

        /// <summary>
        /// 移除一个对象池
        /// </summary>
        /// <param name="name"></param>
        private void Remove(string name)
        {
            if (!m_objectPools.ContainsKey(name))
            {
                Debug.LogError($"ObjectPoolManager: RemovePool: pool {name} is not exist.");
                return;
            }

            ReferencePool.Release(m_objectPools[name]);
            m_objectPools.Remove(name);
        }

        /// <summary>
        /// 获取一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private PoolBase Get(string name)
        {
            if (!m_objectPools.ContainsKey(name))
            {
                Debug.LogError($"ObjectPoolManager: GetPool: pool {name} is not exist.");
                return null;
            }

            return m_objectPools[name];
        }

        /// <summary>
        /// 尝试获取一个对象池
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        private bool TryGet(string name, out PoolBase pool)
        {
            if (!m_objectPools.ContainsKey(name))
            {
                pool = null;
                return false;
            }

            pool = m_objectPools[name];
            return true;
        }

        /// <summary>
        /// 清除所有对象池
        /// </summary>
        private void ClearAll()
        {
            foreach (var objectPool in m_objectPools.Values)
            {
                objectPool.Clear();
            }

            m_objectPools.Clear();
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
        /// 对象池管理器实例
        /// </summary>
        public static ObjectPoolManager Instance => s_instance;

        /// <summary>
        /// 对象池数量
        /// </summary>
        public int PoolCount => m_objectPools.Count;

        /// <summary>
        /// 对象池列表
        /// </summary>
        public IReadOnlyDictionary<string, PoolBase> ObjectPools => m_objectPools;


        private static ObjectPoolManager s_instance;
#if UNITY_EDITOR
        private static ObjectPoolViewer s_viewer;
#endif
        private static GameObject s_managerRoot;
        private static readonly string s_managerRootName = "[ObjectPoolManager]";

        private Dictionary<string, PoolBase> m_objectPools = new Dictionary<string, PoolBase>();
    }
}
