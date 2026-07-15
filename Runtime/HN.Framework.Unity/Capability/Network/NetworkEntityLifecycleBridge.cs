using System.Collections.Generic;
using FishNet.Object;
using HN.Framework.Core.Capability.Network;
using HN.Framework.Core.Driver;
using HN.Framework.Core.Level.Logic.Entity;
using UnityEngine;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 网络实体生命周期桥接组件。
    /// 将 Core 层 <see cref="EntityManager"/> 的实体生命周期事件（Spawn/Despawn/所有权转移）
    /// 桥接到 FishNet 网络层（Spawn/Despawn/GiveOwnership）。
    /// 挂载在 FishNet.NetworkManager 所在的 GameObject 上，仅服务端生效。
    /// </summary>
    public class NetworkEntityLifecycleBridge : MonoBehaviour
    {
        [System.Serializable]
        private class EntityPrefabMapping
        {
            public int EntityDefId;
            public GameObject Prefab;
        }

        [Header("Prefab 注册表 (EntityDefId → Prefab)")]
        [SerializeField]
        private List<EntityPrefabMapping> m_prefabMappings = new List<EntityPrefabMapping>();

        private readonly Dictionary<int, GameObject> m_prefabCache = new Dictionary<int, GameObject>();
        private readonly Dictionary<uint, NetworkEntityView> m_activeViews = new Dictionary<uint, NetworkEntityView>();
        private GameWorld m_world;
        private FishNetNetworkManager m_networkManager;
        private bool m_initialized;

        /// <summary>
        /// 初始化桥接组件，订阅 Core 层实体生命周期事件。
        /// </summary>
        /// <param name="world">GameWorld 实例。</param>
        /// <param name="networkManager">FishNetNetworkManager 包装器。</param>
        public void Initialize(GameWorld world, FishNetNetworkManager networkManager)
        {
            m_world = world;
            m_networkManager = networkManager;
            BuildPrefabCache();

            world.EventBus.Subscribe<EntitySpawnedEvent>(OnEntitySpawned);
            world.EventBus.Subscribe<EntityDespawnedEvent>(OnEntityDespawned);
            world.EventBus.Subscribe<EntityOwnershipTransferredEvent>(OnEntityOwnershipTransferred);

            m_initialized = true;
        }

        /// <summary>
        /// 手动注册预制体映射。可在 Initialize 之前调用以补充序列化映射。
        /// </summary>
        public void RegisterPrefab(int entityDefId, GameObject prefab)
        {
            if (prefab != null && prefab.GetComponent<NetworkObject>() != null)
            {
                m_prefabCache[entityDefId] = prefab;
            }
        }

        private void BuildPrefabCache()
        {
            m_prefabCache.Clear();
            foreach (var mapping in m_prefabMappings)
            {
                if (mapping.Prefab != null && mapping.Prefab.GetComponent<NetworkObject>() != null)
                {
                    m_prefabCache[mapping.EntityDefId] = mapping.Prefab;
                }
            }
        }

        private void OnEntitySpawned(EntitySpawnedEvent evt)
        {
            // 仅服务端触发
            if (!m_initialized || m_networkManager == null || !m_networkManager.IsServer)
                return;

            if (!m_prefabCache.TryGetValue(evt.EntityDefId, out var prefab))
            {
                UnityEngine.Debug.LogWarning($"[NetworkEntityLifecycleBridge] No prefab for EntityDefId={evt.EntityDefId}");
                return;
            }

            var go = Instantiate(prefab);
            var networkView = go.GetComponent<NetworkEntityView>();
            if (networkView == null)
            {
                UnityEngine.Debug.LogError("[NetworkEntityLifecycleBridge] Prefab missing NetworkEntityView");
                Destroy(go);
                return;
            }

            networkView.SetEntityId(evt.EntityId);

            var netObj = go.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                UnityEngine.Debug.LogError("[NetworkEntityLifecycleBridge] Prefab missing NetworkObject");
                Destroy(go);
                return;
            }

            // 查找所有者连接
            FishNet.Connection.NetworkConnection ownerConn = null;
            var entity = m_world.EntityManager.GetEntity(evt.EntityId);
            if (entity != null && entity.OwnerClientId > 0)
            {
                ownerConn = m_networkManager.GetConnection(entity.OwnerClientId);
            }

            if (ownerConn != null)
                FishNet.InstanceFinder.ServerManager.Spawn(netObj, ownerConn);
            else
                FishNet.InstanceFinder.ServerManager.Spawn(netObj);

            m_activeViews[evt.EntityId] = networkView;
        }

        private void OnEntityDespawned(EntityDespawnedEvent evt)
        {
            if (!m_initialized || m_networkManager == null || !m_networkManager.IsServer)
                return;

            if (m_activeViews.TryGetValue(evt.EntityId, out var view) && view.NetworkObject != null)
            {
                FishNet.InstanceFinder.ServerManager.Despawn(view.NetworkObject);
                m_activeViews.Remove(evt.EntityId);
            }
        }

        private void OnEntityOwnershipTransferred(EntityOwnershipTransferredEvent evt)
        {
            if (!m_initialized || m_networkManager == null || !m_networkManager.IsServer)
                return;

            if (!m_activeViews.TryGetValue(evt.EntityId, out var view) || view.NetworkObject == null)
                return;

            var netObj = view.NetworkObject;
            if (evt.NewOwnerId <= 0)
            {
                netObj.RemoveOwnership();
            }
            else
            {
                var conn = m_networkManager.GetConnection(evt.NewOwnerId);
                if (conn != null)
                    netObj.GiveOwnership(conn);
            }
        }

        private void OnDestroy()
        {
            if (m_world?.EventBus != null && m_initialized)
            {
                m_world.EventBus.Unsubscribe<EntitySpawnedEvent>(OnEntitySpawned);
                m_world.EventBus.Unsubscribe<EntityDespawnedEvent>(OnEntityDespawned);
                m_world.EventBus.Unsubscribe<EntityOwnershipTransferredEvent>(OnEntityOwnershipTransferred);
            }
            m_activeViews.Clear();
            m_initialized = false;
        }
    }
}
