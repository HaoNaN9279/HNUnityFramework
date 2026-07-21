using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Core.Driver.Common.Pool.ObjectPool;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Unity.Driver.Platform
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
        /// <param name="settings">GameObject 对象池配置参数</param>
        public override void Initialize(GameObjectPoolSettings settings)
        {
            GenerateRoot(settings.ManagerRoot, settings.BaseSettings.Name);
            this.prototype = settings.Prototype;
            SetName(settings.BaseSettings.Name);
            if (settings.BaseSettings.InitialCount > 0) SetInitialCount(settings.BaseSettings.InitialCount);
            PoolTickFrequency = settings.BaseSettings.TickFrequency;
            PoolMaxCount = settings.BaseSettings.MaxCount;
            PoolMinCount = settings.BaseSettings.MinCount;
            PoolMaxLimitCount = settings.BaseSettings.MaxLimitCount;
            PoolMinLimitCount = settings.BaseSettings.MinLimitCount;
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
                Spawn(out GameObject obj);
            }
        }

        /// <summary>
        /// 从对象池请求一个GameObject
        /// </summary>
        /// <returns></returns>
        public GameObject Acquire(GameObject targetParent = null)
        {
            if (objects.Count == 0)
            {
                if (!Spawn(out _))
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
            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }

        /// <summary>
        /// 从对象池中取出一个GameObject并执行OnAcquire
        /// 执行顺序：SetParent -> OnAcquire -> SetActive
        /// </summary>
        /// <param name="obj"></param>
        public override void OnAcquire(GameObject obj) { }

        /// <summary>
        /// 回收一个GameObject到对象池并执行OnRelease
        /// 执行顺序：SetActive -> OnRelease -> SetParent
        /// </summary>
        /// <param name="obj"></param>
        public override void OnRelease(GameObject obj) { }


        /// <summary>
        /// 由 Tick 获取当前存储数量
        /// </summary>
        protected override int GetStoredCount() => objects.Count;

        /// <summary>
        /// 由 Tick 在数量低于下限时创建一个对象
        /// </summary>
        protected override void TickSpawn() => Spawn(out _);

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
        /// 清空对象池
        /// </summary>
        public override void Clear()
        {
            while (objects.Count > 0)
            {
                Despawn();
            }

            if (Application.isPlaying)
                GameObject.Destroy(root);
            else
                GameObject.DestroyImmediate(root);
            root = null;
            PoolName = string.Empty;
            PoolInitialCount = 0;
            PoolTickFrequency = 0;
            PoolMaxCount = int.MaxValue;
            PoolMaxLimitCount = int.MaxValue;
            PoolMinCount = 0;
            PoolMinLimitCount = 0;
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
    }
}
