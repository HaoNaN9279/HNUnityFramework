using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework.Unity.Level.View
{
    /// <summary>
    /// View 工厂，负责创建和管理 <see cref="EntityView"/> 实例。
    /// 支持同步创建（从已加载的预制体）、预制体缓存和 EntityDefId 映射注册。
    /// </summary>
    public class ViewFactory
    {
        private readonly Dictionary<int, string> m_prefabMappings = new Dictionary<int, string>();
        private readonly Dictionary<string, GameObject> m_prefabCache = new Dictionary<string, GameObject>();

        /// <summary>
        /// 注册实体配置表 ID 到预制体地址的映射。
        /// </summary>
        /// <param name="entityDefId">实体配置表 ID。</param>
        /// <param name="prefabAddress">预制体地址。</param>
        public void RegisterPrefabMapping(int entityDefId, string prefabAddress)
        {
            m_prefabMappings[entityDefId] = prefabAddress;
        }

        /// <summary>
        /// 将预制体添加到缓存，避免重复加载。
        /// </summary>
        /// <param name="address">预制体地址。</param>
        /// <param name="prefab">预制体资源。</param>
        public void CachePrefab(string address, GameObject prefab)
        {
            m_prefabCache[address] = prefab;
        }

        /// <summary>
        /// 从缓存获取预制体。
        /// </summary>
        /// <param name="address">预制体地址。</param>
        /// <returns>缓存的预制体，不存在时返回 null。</returns>
        public GameObject GetCachedPrefab(string address)
        {
            m_prefabCache.TryGetValue(address, out var prefab);
            return prefab;
        }

        /// <summary>
        /// 根据已加载的预制体创建视图实例。
        /// 预制体上必须挂载 <see cref="EntityView"/> 组件。
        /// </summary>
        /// <param name="prefab">已加载的预制体。</param>
        /// <param name="position">世界空间位置。</param>
        /// <param name="rotation">世界空间旋转。</param>
        /// <param name="entityDefId">实体配置表 ID。</param>
        /// <param name="parent">父 Transform。</param>
        /// <returns>创建的 <see cref="EntityView"/> 实例，失败时返回 null。</returns>
        public EntityView CreateView(GameObject prefab, Vector3 position, Quaternion rotation, int entityDefId, Transform parent = null)
        {
            var go = Object.Instantiate(prefab, position, rotation, parent);
            var view = go.GetComponent<EntityView>();
            if (view == null)
            {
                Debug.LogError($"[ViewFactory] Prefab '{prefab.name}' does not have an EntityView component attached.");
                Object.Destroy(go);
                return null;
            }
            view.Initialize(0, entityDefId);
            return view;
        }

        /// <summary>
        /// 释放视图实例，销毁对应的 GameObject。
        /// </summary>
        /// <param name="view">要释放的视图实例。</param>
        public void ReleaseView(EntityView view)
        {
            if (view == null) return;
            view.Deinitialize();
            Object.Destroy(view.gameObject);
        }

        // TODO: CreateViewAsync — 异步加载创建路径，待 IAssetManager 支持返回 GameObject 后实现
    }
}
