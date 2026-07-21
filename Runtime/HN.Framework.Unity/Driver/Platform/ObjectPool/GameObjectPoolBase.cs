using UnityEngine;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

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
        /// <param name="settings">GameObject 对象池配置参数</param>
        public abstract void Initialize(GameObjectPoolSettings settings);

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
