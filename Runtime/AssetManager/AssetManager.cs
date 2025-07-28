using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework
{
    /// <summary>
    /// AssetManager
    /// 依赖 ReferencePool
    /// 负责资源的加载和管理
    /// 通过静态方法提供资源的加载接口
    /// </summary>
    public sealed class AssetManager : ITickable
    {
        #region 对外函数
        /// <summary>
        /// 初始化AssetManager
        /// </summary>
        public static void Initialize()
        {
            // 检查是否使用Addressables
            UseAddressables = false;
            if (System.Type.GetType("UnityEngine.AddressableAssets.Addressables, Unity.Addressables") != null)
            {
                UseAddressables = true;
            }

            // 初始化Addressables操作器
            if (UseAddressables)
            {
                Instance.m_addressablesOperator = ReferencePool.Acquire<AddressablesOperator>();
            }
            else
            {
                Instance.m_resourcesOperator = ReferencePool.Acquire<ResourcesOperator>();
            }
#if UNITY_EDITOR
            Instance.m_assetDatabaseOperator = ReferencePool.Acquire<AssetDatabaseOperator>();
#endif
        }

        public static void Uninitialize()
        {
            if (Instance == null)
            {
                return;
            }

            ReferencePool.Release(Instance.m_loadedAssets);
            if (UseAddressables)
            {
                ReferencePool.Release(Instance.m_addressablesOperator);
            }
            else
            {
                ReferencePool.Release(Instance.m_resourcesOperator);
            }
#if UNITY_EDITOR
            ReferencePool.Release(Instance.m_assetDatabaseOperator);
#endif
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Object Load(string name) => Instance.LoadAsset(name);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static AsyncLoadHandle LoadAsync<T>(string name) where T : Object => Instance.LoadAssetAsync<T>(name);

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        public static void Release<T>(T asset) where T : Object => Instance.ReleaseAsset(asset);

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="asset"></param>
        public static void Release(Object asset) => Instance.ReleaseAsset(asset);

        /// <summary>
        /// 每帧更新AssetManager
        /// </summary>
        public static void TickAssetManager() => Instance.Tick();

        /// <summary>
        /// 每帧后更新AssetManager
        /// </summary>
        public static void LateTickAssetManager() => Instance.LateTick();
        #endregion

        #region 实现接口 ITickable
        /// <summary>
        /// 每帧更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Tick()
        {
            CleanEmptyCache();
        }

        /// <summary>
        /// 每帧后更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void LateTick()
        {

        }
        #endregion

        #region 私有函数
        private Object LoadAsset(string name)
        {
            // 检查是否已经加载该资源
            if (m_loadedAssets.ContainsKey(name) && m_loadedAssets[name].IsAlive)
            {
                return m_loadedAssets[name].Asset;
            }

            // 加载资源
            if (name.StartsWith("Assets/"))
            {
#if UNITY_EDITOR
                var asset = m_assetDatabaseOperator.LoadAsset(name);
                var assetCacheItem = ReferencePool.Acquire<AssetCacheItem>();
                assetCacheItem.Initialize(name, asset);
                m_loadedAssets[name] = assetCacheItem;
                return asset;
#endif
                Debug.LogError("Use Addressables or Resources.LoadAsync to load assets from Assets folder.");
                return null;
            }
            else
            {
                var asset = m_resourcesOperator.LoadAsset(name);
                var assetCacheItem = ReferencePool.Acquire<AssetCacheItem>();
                assetCacheItem.Initialize(name, asset);
                m_loadedAssets[name] = assetCacheItem;
                return asset;
            }
        }

        private AsyncLoadHandle LoadAssetAsync<T>(string name) where T : Object
        {
            // 检查是否已经加载该资源
            if (m_loadedAssets.ContainsKey(name) && m_loadedAssets[name].IsAlive)
            {
                return new AsyncLoadHandle(m_loadedAssets[name].Asset as Object);
            }

            // 加载资源
            if (UseAddressables)
            {
                var handle = m_addressablesOperator.LoadAssetAsync<T>(name);
                var assetCacheItem = ReferencePool.Acquire<AssetCacheItem>();
                assetCacheItem.Initialize(name, handle.Target as Object);
                m_loadedAssets[name] = assetCacheItem;
                return handle;
            }
            else
            {
                if (!name.StartsWith("Assets/"))
                {
                    var handle = m_resourcesOperator.LoadAssetAsync<T>(name);
                var assetCacheItem = ReferencePool.Acquire<AssetCacheItem>();
                assetCacheItem.Initialize(name, handle.Target as Object);
                m_loadedAssets[name] = assetCacheItem;
                    return handle;
                }
                else
                {
#if UNITY_EDITOR
                    var handle = new AsyncLoadHandle(m_assetDatabaseOperator.LoadAsset(name));
                    var assetCacheItem = ReferencePool.Acquire<AssetCacheItem>();
                    assetCacheItem.Initialize(name, handle.Target as Object);
                    m_loadedAssets[name] = assetCacheItem;
                    return handle;
#endif
                    Debug.LogError("Resources.LoadAsync does not support loading from Assets folder directly. Please use Addressables or load from Resources folder.");
                    return null;
                }
            }
        }

        private void ReleaseAsset<T>(T asset) where T : Object
        {
            if (UseAddressables)
            {
#if UNITY_EDITOR
                asset = null;
                return;
#endif
                s_instance.m_addressablesOperator.ReleaseAsset(asset);
            }
            else
            {
                s_instance.m_resourcesOperator.ReleaseAsset(asset);
            }
        }

        private void CleanEmptyCache()
        {
            var enptyCacheKeys = ReferencePool.Acquire<PooledList<string>>();
            // 遍历已加载的资源，检查是否有无效的弱引用
            foreach (var kvp in m_loadedAssets)
            {
                if (!kvp.Value.IsAlive)
                {
                    enptyCacheKeys.Add(kvp.Key);
                }
            }
            // 清理无效的资源缓存项
            foreach (var key in enptyCacheKeys)
            {
                ReferencePool.Release(m_loadedAssets[key]);
                m_loadedAssets.Remove(key);
            }
            ReferencePool.Release(enptyCacheKeys);
        }
        #endregion


        #region 对外属性
        /// <summary>
        /// 是否使用Addressables
        /// </summary>
        public static bool UseAddressables = false;
        
        /// <summary>
        /// 获取当前加载的资源列表
        /// 资源列表是一个字典，键为资源名称，值为弱引用
        /// </summary>
        public static IReadOnlyDictionary<string, AssetCacheItem> LoadedAssets => s_instance.m_loadedAssets;
        #endregion

        #region 私有成员
        private static AssetManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new AssetManager();
                }
                return s_instance;
            }
        }
        private static AssetManager s_instance;

        private AddressablesOperator m_addressablesOperator;
        private ResourcesOperator m_resourcesOperator;
#if UNITY_EDITOR
        private AssetDatabaseOperator m_assetDatabaseOperator;
#endif
        private PooledDictionary<string, AssetCacheItem> m_loadedAssets = ReferencePool.Acquire<PooledDictionary<string, AssetCacheItem>>();
        #endregion
    }
}
