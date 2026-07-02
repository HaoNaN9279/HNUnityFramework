using UnityEngine;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Unity.Driver.Platform
{
    /// <summary>
    /// UnityEngine.GameObject 对象池抽象基类
    /// </summary>
    public abstract class GameObjectPoolBase : PoolBase
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency, int maxCount, int minCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public abstract void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount);

        /// <summary>
        /// 从对象池中取出一个GameObject并执行OnAcquire
        /// 执行顺序：SetParent -> OnAcquire -> SetActive
        /// </summary>
        /// <param name="obj"></param>
        public abstract void OnAcquire(GameObject obj);

        /// <summary>
        /// 回收一个GameObject到对象池并执行OnRelease
        /// 执行顺序：SetActive -> OnRelease -> SetParent
        /// </summary>
        /// <param name="obj"></param>
        public abstract void OnRelease(GameObject obj);

        /// <summary>
        /// 生成根节点
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="name"></param>
        protected void GenerateRoot(GameObject managerRoot, string name)
        {
            if (managerRoot == null)
            {
                Debug.LogError("Manager Root is Empty.");
                return;
            }

            GameObject root = new GameObject($"[{name}]");
            root.transform.parent = managerRoot.transform;
            this.root = root;
        }

        /// <summary>
        /// 原型
        /// </summary>
        protected GameObject prototype;

        /// <summary>
        /// 根节点
        /// </summary>
        protected GameObject root;

        /// <summary>
        /// GameObject 对象队列
        /// </summary>
        protected PooledQueue<GameObject> objects = new PooledQueue<GameObject>();
    }
}
