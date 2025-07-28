using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    /// <summary>
    /// UnityEngine.GameObject 对象池接口
    /// </summary>
    public interface IGameObjectPool
    {
        /// <summary>
        /// 从对象池请求一个GameObject
        /// </summary>
        /// <returns></returns>
        public GameObject Acquire(GameObject targetParent = null);

        /// <summary>
        /// 回收一个GameObject到对象池
        /// </summary>
        /// <param name="obj"></param>
        public void Release(GameObject obj);

        /// <summary>
        /// 创建一个GameObject到对象池
        /// </summary>
        /// <returns></returns>
        public bool Spawn(out GameObject obj);

        /// <summary>
        /// 从对象池取出一个GameObject并销毁
        /// </summary>
        public void Despawn();
    }


    /// <summary>
    /// UnityEngine.GameObject 对象池
    /// </summary>
    public abstract class GameObjectPool : GameObjectPoolBase, IGameObjectPool
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            this.tickFrequency = tickFrequency;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency, int maxCount, int minCount)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
        }

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
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
            this.maxLimitCount = maxLimitCount;
            this.minLimitCount = minLimitCount;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="managerRoot"></param>
        /// <param name="prototype"></param>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            SetInitialCount(initialCount);
            this.tickFrequency = tickFrequency;
        }

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
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            SetInitialCount(initialCount);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
        }

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
        public override void Initialize(GameObject managerRoot, GameObject prototype, string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount)
        {
            GenerateRoot(managerRoot, name);
            this.prototype = prototype;
            SetName(name);
            SetInitialCount(initialCount);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
            this.maxLimitCount = maxLimitCount;
            this.minLimitCount = minLimitCount;
        }

        /// <summary>
        /// 设置对象池初始数量并填充
        /// </summary>
        /// <param name="initialCount"></param>
        public override void SetInitialCount(int initialCount)
        {
            if (initialCount <= 0)
            {
                return;
            }

            this.initialCount = initialCount;
            for (int i = 0; i < initialCount; i++)
            {
                Spawn(out GameObject obj);
            }
        }

        /// <summary>
        /// 从对象池请求一个GameObject
        /// </summary>
        /// <returns></returns>
        public GameObject Acquire(GameObject targetParent = null)
        {
            bool isFaild = false;
            if (objects.Count == 0)
            {
                isFaild = !Spawn(out GameObject obj);
            }

            if (isFaild)
            {
                return null;
            }

            GameObject target = objects.Dequeue();
            if (targetParent != null)
            {
                target.transform.parent = targetParent.transform;
            }
            else
            {
                target.transform.parent = null;
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(target, activeScene);
            }
            OnAcquire(target);
            target.SetActive(true);
            return target;
        }

        /// <summary>
        /// 回收一个GameObject到对象池
        /// </summary>
        /// <param name="obj"></param>
        public void Release(GameObject obj)
        {
            obj.SetActive(false);
            OnRelease(obj);
            obj.transform.parent = root.transform;
            objects.Enqueue(obj);
        }

        /// <summary>
        /// 创建一个GameObject到对象池
        /// </summary>
        /// <returns></returns>
        public bool Spawn(out GameObject obj)
        {
            if (prototype == null)
            {
                obj = null;
                return false;
            }

            obj = GameObject.Instantiate<GameObject>(prototype);
            obj.transform.parent = root.transform;
            obj.SetActive(false);
            objects.Enqueue(obj);
            return true;
        }

        /// <summary>
        /// 从对象池取出一个GameObject并销毁
        /// </summary>
        public void Despawn()
        {
            if (objects.Count == 0)
            {
                return;
            }
            GameObject obj = objects.Dequeue();
            GameObject.Destroy(obj);
        }

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
        /// 每帧更新
        /// </summary>
        public override void Tick()
        {
            // 每隔tickFrequency帧执行一次
            if (Time.frameCount % tickFrequency != 0)
            {
                return;
            }

            // 超过上限开始销毁
            if (objects.Count > maxCount && objects.Count < maxLimitCount)
            {
                Despawn();
            }
            else if (objects.Count > maxLimitCount)
            {
                for (int i = 0; i < objects.Count - maxLimitCount; i++)
                {
                    Despawn();
                }
            }

            // 小于下限开始创建
            if (objects.Count < minCount && objects.Count > minLimitCount)
            {
                Spawn(out GameObject obj);
            }
            else if (objects.Count < minLimitCount)
            {
                for (int i = 0; i < minLimitCount - objects.Count; i++)
                {
                    Spawn(out GameObject obj);
                }
            }
        }

        /// <summary>
        /// 每帧后更新
        /// </summary>
        public override void LateTick()
        {

        }

        /// <summary>
        /// 清空对象池
        /// </summary>
        public override void Clear()
        {
            while (objects.Count > 0)
            {
                Despawn();
            }

            AssetManager.Release(prototype);
            GameObject.Destroy(root);
            prototype = null;
            root = null;
            name = string.Empty;
            initialCount = 0;
            tickFrequency = 0;
            maxCount = int.MaxValue;
            maxLimitCount = int.MaxValue;
            minCount = 0;
            minLimitCount = 0;
        }

        /// <summary>
        /// 获取当前数量
        /// </summary>
        /// <returns></returns>
        public override int GetCurrentCount()
        {
            return objects.Count;
        }

        /// <summary>
        /// 获取对象类型
        /// </summary>
        /// <returns></returns>
        public override System.Type GetObjectType()
        {
            return typeof(GameObject);
        }

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
