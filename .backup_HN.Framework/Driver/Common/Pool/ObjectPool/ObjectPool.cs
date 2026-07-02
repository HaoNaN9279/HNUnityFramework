using System.Collections;
using System.Collections.Generic;

namespace HN.Framework.Driver.Common
{
    /// <summary>
    /// UnityEngine.Object 对象池接口
    /// </summary>
    public interface IObjectPool<T> where T : PooledObjectBase, new()
    {
        /// <summary>
        /// 从对象池请求一个Object
        /// </summary>
        /// <returns></returns>
        public T Acquire();

        /// <summary>
        /// 回收一个Object到对象池
        /// </summary>
        /// <param name="obj"></param>
        public void Release(T obj);


        /// <summary>
        /// 创建一个Object并放入对象池
        /// </summary>
        public void Spawn();

        /// <summary>
        /// 从对象池取出一个Object并销毁
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn();
    }


    /// <summary>
    /// UnityEngine.Object 对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObjectPool<T> : ObjectPoolBase, IObjectPool<T> where T : PooledObjectBase, new()
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        public override void Initialize(string name)
        {
            SetName(name);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        public override void Initialize(string name, int tickFrequency)
        {
            SetName(name);
            this.tickFrequency = tickFrequency;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public override void Initialize(string name, int tickFrequency, int maxCount, int minCount)
        {
            SetName(name);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public override void Initialize(string name, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount)
        {
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
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        public override void Initialize(string name, int initialCount, int tickFrequency)
        {
            SetName(name);
            SetInitialCount(initialCount);
            this.tickFrequency = tickFrequency;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        public override void Initialize(string name, int initialCount, int tickFrequency, int maxCount, int minCount)
        {
            SetName(name);
            SetInitialCount(initialCount);
            this.tickFrequency = tickFrequency;
            this.maxCount = maxCount;
            this.minCount = minCount;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="name"></param>
        /// <param name="initialCount"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="maxCount"></param>
        /// <param name="minCount"></param>
        /// <param name="maxLimitCount"></param>
        /// <param name="minLimitCount"></param>
        public override void Initialize(string name, int initialCount, int tickFrequency, int maxCount, int minCount, int maxLimitCount, int minLimitCount)
        {
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
                Spawn();
            }
        }

        /// <summary>
        /// 从对象池请求一个Object
        /// </summary>
        /// <returns></returns>
        public T Acquire()
        {
            if (objects.Count == 0)
            {
                Spawn();
            }

            return objects.Dequeue() as T;
        }

        /// <summary>
        /// 回收一个Object到对象池
        /// </summary>
        /// <param name="pooledObject"></param>
        public void Release(T pooledObject)
        {
            objects.Enqueue(pooledObject);
        }

        /// <summary>
        /// 创建一个Object并放入对象池
        /// </summary>
        public abstract void Spawn();

        /// <summary>
        /// 从对象池取出一个Object并销毁
        /// </summary>
        /// <param name="obj"></param>
        public abstract void Despawn();

        /// <summary>
        /// 每帧更新
        /// </summary>
        public override void Tick()
        {
            // 每隔tickFrequency帧执行一次
            if (tickFrequency != 0 && HNLogicTime.LogicFrameCount % (ulong)tickFrequency != 0)
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
                Spawn();
            }
            else if (objects.Count < minLimitCount)
            {
                for (int i = 0; i < minLimitCount - objects.Count; i++)
                {
                    Spawn();
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
            return typeof(T);
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

            name = string.Empty;
            initialCount = 0;
            tickFrequency = 0;
            maxCount = int.MaxValue;
            maxLimitCount = int.MaxValue;
            minCount = 0;
            minLimitCount = 0;
        }


        /// <summary>
        /// Object 对象队列
        /// </summary>
        protected PooledQueue<PooledObjectBase> objects = new PooledQueue<PooledObjectBase>();
    }


}
