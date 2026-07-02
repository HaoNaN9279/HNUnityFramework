using System;
using System.Collections.Generic;
using System.Linq;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common;
using HN.Framework.Unity.Driver.Platform;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Asset
{
    /// <summary>
    /// 资源管理器，提供资源加载、缓存、引用计数与场景组管理功能。
    /// 实现 ITickable 以支持每帧更新，实现 IAssetManager 以提供统一资源管理接口。
    /// </summary>
    public sealed class AssetManager : ITickable, IAssetManager
    {
        /// <summary>
        /// 资源加载记录，记录单个资源的引用计数、最近访问时间及所属场景组。
        /// </summary>
        internal struct AssetLoadRecord
        {
            /// <summary>
            /// 资源键。
            /// </summary>
            public string Key;

            /// <summary>
            /// 资源对象。
            /// </summary>
            public object Asset;

            /// <summary>
            /// 引用计数。
            /// </summary>
            public int RefCount;

            /// <summary>
            /// 最近一次访问时间（Time.unscaledTime）。
            /// </summary>
            public float LastAccessTime;

            /// <summary>
            /// 所属场景组标签列表。
            /// </summary>
            public List<string> SceneLabels;
        }

        /// <summary>
        /// 预加载请求，记录预加载任务及其进度回调。
        /// </summary>
        internal struct PreloadRequest
        {
            /// <summary>
            /// 场景组标签。
            /// </summary>
            public string Label;

            /// <summary>
            /// 预加载进度回调（0.0 ~ 1.0）。
            /// </summary>
            public Action<float> OnProgress;
        }

        private readonly Dictionary<string, AssetLoadRecord> m_LoadRecords = new Dictionary<string, AssetLoadRecord>();
        private readonly Dictionary<string, HashSet<string>> m_SceneGroups = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, AssetCacheItem> m_AssetCache = new Dictionary<string, AssetCacheItem>();
        private readonly Queue<PreloadRequest> m_PreloadQueue = new Queue<PreloadRequest>();
        private readonly Dictionary<string, (int loaded, int total)> m_GroupProgress = new Dictionary<string, (int loaded, int total)>();

        private float m_AutoUnloadDelay = 30f;
        private IAssetOperator m_Operator;

        /// <summary>
        /// 设置资源操作器。
        /// </summary>
        /// <param name="op">资源操作器实例。</param>
        public void SetOperator(IAssetOperator op)
        {
            m_Operator = op;
        }

        /// <summary>
        /// 每帧更新。处理过期的资源自动卸载、预加载队列消费以及缓存失效项回收。
        /// </summary>
        public void Tick()
        {
            // 1. Auto-unload expired assets (RefCount == 0 && beyond delay threshold)
            if (m_Operator == null)
            {
                Debug.LogWarning("[AssetManager] Tick: m_Operator is null, cannot perform auto-unload.");
            }
            else
            {
                foreach (var kvp in m_LoadRecords.ToArray())
                {
                    var record = kvp.Value;
                    if (record.RefCount == 0 && (Time.time - record.LastAccessTime) > m_AutoUnloadDelay)
                    {
                        var key = kvp.Key;

                        // Release the underlying asset via operator
                        var unityAsset = record.Asset as UnityEngine.Object;
                        if (unityAsset != null)
                        {
                            m_Operator.ReleaseAsset(unityAsset);
                        }

                        // Remove from load records
                        m_LoadRecords.Remove(key);

                        // Return alive cache item to pool and remove from cache
                        if (m_AssetCache.TryGetValue(key, out var cacheItem))
                        {
                            if (cacheItem.IsAlive)
                            {
                                ReferencePool.Release(cacheItem);
                            }
                            m_AssetCache.Remove(key);
                        }
                    }
                }
            }

            // 2. Preload processing (rate-limited: max 1 per Tick)
            if (m_PreloadQueue.Count > 0)
            {
                var request = m_PreloadQueue.Dequeue();
                LoadSceneGroup(request.Label, request.OnProgress);
            }

            // 3. Cache eviction — remove dead weak-ref entries
            foreach (var kvp in m_AssetCache.ToArray())
            {
                var cacheItem = kvp.Value;
                if (!cacheItem.IsAlive)
                {
                    ReferencePool.Release(cacheItem);
                    m_AssetCache.Remove(kvp.Key);
                }
            }
        }

        /// <summary>
        /// 每帧后更新。
        /// </summary>
        public void LateTick()
        {
        }

        /// <summary>
        /// 加载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <param name="onProgress">加载进度回调（0.0 ~ 1.0），可选。</param>
        public void LoadSceneGroup(string label, Action<float> onProgress = null)
        {
            // 查找 m_LoadRecords 中 SceneLabels 包含指定标签的所有记录
            var groupKeys = m_LoadRecords
                .Where(kvp => kvp.Value.SceneLabels != null && kvp.Value.SceneLabels.Contains(label))
                .Select(kvp => kvp.Key)
                .ToList();

            if (groupKeys.Count == 0)
                return;

            // 注册到场景组索引（供 GetGroupProgress 使用）
            if (!m_SceneGroups.TryGetValue(label, out var keys))
            {
                keys = new HashSet<string>();
                m_SceneGroups[label] = keys;
            }
            foreach (var key in groupKeys)
            {
                keys.Add(key);
            }

            // 注册进度追踪（供 GetGroupProgress 使用）
            int loadedCount = 0;
            int totalCount = groupKeys.Count;
            m_GroupProgress[label] = (0, totalCount);

            // 逐个加载并报告进度
            foreach (var key in groupKeys)
            {
                LoadAsset(key);
                loadedCount++;
                m_GroupProgress[label] = (loadedCount, totalCount);
                onProgress?.Invoke(loadedCount / (float)totalCount);
            }

            onProgress?.Invoke(1.0f);
        }

        /// <summary>
        /// 卸载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        public void UnloadSceneGroup(string label)
        {
            // 查找 m_LoadRecords 中 SceneLabels 包含指定标签的所有记录
            var groupKeys = m_LoadRecords
                .Where(kvp => kvp.Value.SceneLabels != null && kvp.Value.SceneLabels.Contains(label))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in groupKeys)
            {
                ReleaseAsset(key);

                // 从记录的 SceneLabels 中移除该标签
                if (m_LoadRecords.TryGetValue(key, out var record) && record.SceneLabels != null)
                {
                    record.SceneLabels.Remove(label);
                    m_LoadRecords[key] = record;
                }
            }

            // 从场景组索引中移除
            if (m_SceneGroups.ContainsKey(label))
            {
                m_SceneGroups.Remove(label);
            }
        }

        /// <summary>
        /// 预加载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <param name="onProgress">预加载进度回调（0.0 ~ 1.0），可选。</param>
        public void PreloadSceneGroup(string label, Action<float> onProgress = null)
        {
            m_PreloadQueue.Enqueue(new PreloadRequest
            {
                Label = label,
                OnProgress = onProgress
            });
        }

        /// <summary>
        /// 加载指定键的资源。
        /// </summary>
        /// <param name="key">资源键。</param>
        public void LoadAsset(string key)
        {
            if (m_AssetCache.TryGetValue(key, out var cacheItem) && cacheItem.IsAlive)
            {
                // 缓存命中，增加引用计数并更新访问时间
                var record = m_LoadRecords[key];
                record.RefCount++;
                record.LastAccessTime = Time.time;
                m_LoadRecords[key] = record;
                return;
            }

            var asset = m_Operator.LoadAsset(key);
            if (asset == null)
            {
                Debug.LogWarning($"[AssetManager] LoadAsset failed: key '{key}' returned null.");
                return;
            }

            // 从引用池获取缓存项并初始化
            var newCacheItem = ReferencePool.Acquire<AssetCacheItem>();
            newCacheItem.Initialize(key, asset);
            m_AssetCache[key] = newCacheItem;

            // 创建或更新加载记录
            TryGetOrCreateRecord(key, out var newRecord);
            newRecord.Key = key;
            newRecord.Asset = asset;
            newRecord.RefCount = 1;
            newRecord.LastAccessTime = Time.time;
            m_LoadRecords[key] = newRecord;
        }

        /// <summary>
        /// 释放指定键的资源。
        /// </summary>
        /// <param name="key">资源键。</param>
        public void ReleaseAsset(string key)
        {
            if (m_LoadRecords.TryGetValue(key, out var record))
            {
                record.RefCount--;
                if (record.RefCount == 0)
                {
                    record.LastAccessTime = Time.time;
                }

                m_LoadRecords[key] = record;
            }
            else
            {
                Debug.LogWarning($"AssetManager.ReleaseAsset: key '{key}' not found.");
            }
        }

        /// <summary>
        /// 检查指定键的资源是否已加载。
        /// </summary>
        /// <param name="key">资源键。</param>
        /// <returns>如果资源已加载则返回 true，否则返回 false。</returns>
        public bool IsLoaded(string key)
        {
            return m_LoadRecords.TryGetValue(key, out var record) && record.RefCount > 0;
        }

        /// <summary>
        /// 获取指定键资源的引用计数。
        /// </summary>
        /// <param name="key">资源键。</param>
        /// <returns>资源的引用计数。</returns>
        public int GetReferenceCount(string key)
        {
            return m_LoadRecords.TryGetValue(key, out var record) ? record.RefCount : 0;
        }

        /// <summary>
        /// 获取指定标签资源组的加载进度。
        /// 优先从 <see cref="m_SceneGroups"/> 计算精确进度（基于 IsLoaded），
        /// 若未注册则回退到 <see cref="m_GroupProgress"/> 获取加载过程中的追踪值。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <returns>资源组加载进度（0.0 ~ 1.0），未找到时返回 0f。</returns>
        public float GetGroupProgress(string label)
        {
            if (m_SceneGroups.TryGetValue(label, out var keys))
            {
                int loadedCount = keys.Count(key => IsLoaded(key));
                return loadedCount / (float)keys.Count;
            }

            if (m_GroupProgress.TryGetValue(label, out var progress))
            {
                return progress.loaded / (float)progress.total;
            }

            return 0f;
        }

        /// <summary>
        /// 获取当前已加载的资源数量。
        /// </summary>
        /// <returns>已加载的资源数量。</returns>
        public int GetLoadedAssetCount()
        {
            return m_LoadRecords.Values.Count(record => record.RefCount > 0);
        }

        /// <summary>
        /// 清空所有资源记录、缓存、场景组及进度追踪，重置自动卸载延迟。
        /// </summary>
        public void Clear()
        {
            // 释放所有仍有引用的资源
            foreach (var kvp in m_LoadRecords.ToArray())
            {
                if (kvp.Value.RefCount > 0)
                {
                    ReleaseAsset(kvp.Key);
                }
            }

            // 回收所有存活的缓存项
            foreach (var cacheItem in m_AssetCache.Values)
            {
                if (cacheItem.IsAlive)
                {
                    ReferencePool.Release(cacheItem);
                }
            }

            // 清空所有字典和队列
            m_LoadRecords.Clear();
            m_SceneGroups.Clear();
            m_AssetCache.Clear();
            m_GroupProgress.Clear();
            m_PreloadQueue.Clear();

            // 恢复默认值
            m_AutoUnloadDelay = 30f;

            Debug.Log("[AssetManager] Cleared all resources.");
        }

        /// <summary>
        /// 设置自动卸载延迟时间（供测试使用）。
        /// </summary>
        /// <param name="seconds">延迟秒数。</param>
        internal void SetAutoUnloadDelay(float seconds)
        {
            m_AutoUnloadDelay = seconds;
        }

        /// <summary>
        /// 当前待处理的预加载请求数量（供测试使用）。
        /// </summary>
        internal int PendingPreloadCount => m_PreloadQueue.Count;

        /// <summary>
        /// 尝试获取指定键的资源加载记录，若不存在则创建一条新记录。
        /// </summary>
        /// <param name="key">资源键。</param>
        /// <param name="record">获取或创建的加载记录。</param>
        /// <returns>如果记录已存在则返回 true，否则返回 false。</returns>
        private bool TryGetOrCreateRecord(string key, out AssetLoadRecord record)
        {
            if (m_LoadRecords.TryGetValue(key, out record))
            {
                return true;
            }

            record = new AssetLoadRecord
            {
                Key = key,
                Asset = null,
                RefCount = 0,
                LastAccessTime = 0,
                SceneLabels = new List<string>()
            };
            m_LoadRecords[key] = record;
            return false;
        }
    }
}
