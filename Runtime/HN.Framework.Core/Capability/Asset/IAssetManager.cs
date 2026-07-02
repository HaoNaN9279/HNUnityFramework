using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Capability
{
    /// <summary>
    /// 资源管理器接口，定义资源组和单个资源的加载、卸载与查询操作。
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// 加载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <param name="onProgress">加载进度回调（0.0 ~ 1.0），可选。</param>
        void LoadSceneGroup(string label, Action<float> onProgress = null);

        /// <summary>
        /// 卸载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        void UnloadSceneGroup(string label);

        /// <summary>
        /// 预加载指定标签的资源组。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <param name="onProgress">预加载进度回调（0.0 ~ 1.0），可选。</param>
        void PreloadSceneGroup(string label, Action<float> onProgress = null);

        /// <summary>
        /// 加载指定键的资源。
        /// </summary>
        /// <param name="key">资源键。</param>
        void LoadAsset(string key);

        /// <summary>
        /// 释放指定键的资源。
        /// </summary>
        /// <param name="key">资源键。</param>
        void ReleaseAsset(string key);

        /// <summary>
        /// 检查指定键的资源是否已加载。
        /// </summary>
        /// <param name="key">资源键。</param>
        /// <returns>如果资源已加载则返回 true，否则返回 false。</returns>
        bool IsLoaded(string key);

        /// <summary>
        /// 获取指定键资源的引用计数。
        /// </summary>
        /// <param name="key">资源键。</param>
        /// <returns>资源的引用计数。</returns>
        int GetReferenceCount(string key);

        /// <summary>
        /// 获取指定标签资源组的加载进度。
        /// </summary>
        /// <param name="label">资源组标签。</param>
        /// <returns>资源组加载进度（0.0 ~ 1.0）。</returns>
        float GetGroupProgress(string label);

        /// <summary>
        /// 获取当前已加载的资源数量。
        /// </summary>
        /// <returns>已加载的资源数量。</returns>
        int GetLoadedAssetCount();

        /// <summary>
        /// 每帧更新。处理过期的资源自动卸载、预加载队列消费以及缓存失效项回收。
        /// </summary>
        void Tick();

        /// <summary>
        /// 每帧后更新。
        /// </summary>
        void LateTick();
    }
}
