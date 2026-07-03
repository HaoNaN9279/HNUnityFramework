using System;
using System.Collections;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Driver.Common.Pool.ObjectPool
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
        /// <param name="settings">对象池配置参数</param>
        public override void Initialize(PoolSettings settings)
        {
            SetName(settings.Name);
            if (settings.InitialCount > 0) SetInitialCount(settings.InitialCount);
            PoolTickFrequency = settings.TickFrequency;
            PoolMaxCount = settings.MaxCount;
            PoolMinCount = settings.MinCount;
            PoolMaxLimitCount = settings.MaxLimitCount;
            PoolMinLimitCount = settings.MinLimitCount;
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

            PoolInitialCount = initialCount;
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

            return (T)objects.Dequeue();
        }

        /// <summary>
        /// 回收一个Object到对象池
        /// </summary>
        /// <param name="pooledObject"></param>
        public void Release(T pooledObject)
        {
            if (pooledObject == null)
            {
                throw new ArgumentNullException(nameof(pooledObject));
            }

            pooledObject.Clear();

            if (objects.Contains(pooledObject))
            {
                throw new InvalidOperationException("Object has already been released to the pool.");
            }

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
        /// 由 Tick 获取当前存储数量
        /// </summary>
        protected override int GetStoredCount() => objects.Count;

        /// <summary>
        /// 由 Tick 在数量低于下限时创建一个对象
        /// </summary>
        protected override void TickSpawn() => Spawn();

        /// <summary>
        /// 由 Tick 在数量高于上限时销毁一个对象
        /// </summary>
        protected override void TickDespawn() => Despawn();

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

            PoolName = string.Empty;
            PoolInitialCount = 0;
            PoolTickFrequency = 0;
            PoolMaxCount = int.MaxValue;
            PoolMaxLimitCount = int.MaxValue;
            PoolMinCount = 0;
            PoolMinLimitCount = 0;
        }


        /// <summary>
        /// Object 对象队列
        /// </summary>
        protected PooledQueue<PooledObjectBase> objects = new PooledQueue<PooledObjectBase>();
    }


}
