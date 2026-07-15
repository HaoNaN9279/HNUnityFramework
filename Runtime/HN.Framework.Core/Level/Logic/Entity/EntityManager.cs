using System.Collections.Generic;
using HN.Framework.Core.Capability.Event;
using HN.Framework.Core.Driver.Common.Pool.ReferencePool;

namespace HN.Framework.Core.Level.Logic.Entity
{
    /// <summary>
    /// 实体管理器，负责管理 Entity 的创建、销毁和查询。
    /// 通过 <see cref="EventBus"/> 发布实体生成/销毁事件，
    /// 供 Unity 层监听以创建/销毁对应的 GameObject。
    /// </summary>
    public sealed class EntityManager
    {
        private readonly EventBus m_eventBus;
        private readonly Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
        private uint m_nextEntityId = 1;

        /// <summary>
        /// 初始化实体管理器。
        /// </summary>
        /// <param name="eventBus">事件总线，实体管理器通过它发布实体生命周期事件。</param>
        public EntityManager(EventBus eventBus)
        {
            m_eventBus = eventBus;
        }

        /// <summary>
        /// 生成指定配置类型的实体。
        /// </summary>
        /// <param name="entityDefId">实体配置表 ID。</param>
        /// <returns>生成的实体实例。</returns>
        public Entity Spawn(int entityDefId)
        {
            return SpawnWithOwner(entityDefId, -1);
        }

        /// <summary>
        /// 生成指定配置类型并指定网络归属的实体。
        /// </summary>
        /// <param name="entityDefId">实体配置表 ID。</param>
        /// <param name="ownerClientId">拥有该实体的客户端 ID，-1 表示无归属。</param>
        /// <returns>生成的实体实例。</returns>
        public Entity SpawnWithOwner(int entityDefId, int ownerClientId)
        {
            var entity = ReferencePool.Acquire<Entity>();
            uint id = m_nextEntityId++;
            entity.Initialize(id, entityDefId, ownerClientId);
            m_entities.Add(id, entity);
            m_eventBus.Publish(new EntitySpawnedEvent(id, entityDefId));
            return entity;
        }

        /// <summary>
        /// 销毁指定 ID 的实体。
        /// </summary>
        /// <param name="entityId">实体 ID。</param>
        public void Despawn(uint entityId)
        {
            if (m_entities.TryGetValue(entityId, out var entity))
            {
                m_entities.Remove(entityId);
                entity.Clear();
                ReferencePool.Release(entity);
                m_eventBus.Publish(new EntityDespawnedEvent(entityId));
            }
        }

        /// <summary>
        /// 获取指定 ID 的实体。
        /// </summary>
        /// <param name="entityId">实体 ID。</param>
        /// <returns>实体实例，未找到时返回 null。</returns>
        public Entity GetEntity(uint entityId)
        {
            m_entities.TryGetValue(entityId, out var entity);
            return entity;
        }

        /// <summary>
        /// 检查指定客户端是否拥有指定实体的操作权限。
        /// </summary>
        /// <param name="entityId">实体 ID。</param>
        /// <param name="clientId">客户端 ID，0 表示服务器，始终拥有全部权限。</param>
        /// <returns>如果客户端拥有该实体的操作权限则返回 true。</returns>
        public bool HasAuthority(uint entityId, int clientId)
        {
            if (m_entities.TryGetValue(entityId, out var entity))
            {
                if (clientId == 0)
                    return true;

                return entity.OwnerClientId == clientId;
            }

            return false;
        }

        /// <summary>
        /// 将指定实体的所有权转移给新的客户端。
        /// </summary>
        /// <param name="entityId">实体 ID。</param>
        /// <param name="newOwnerId">新拥有者的客户端 ID，-1 表示移除所有权。</param>
        public void TransferOwnership(uint entityId, int newOwnerId)
        {
            if (m_entities.TryGetValue(entityId, out var entity))
            {
                int oldOwnerId = entity.OwnerClientId;
                entity.OwnerClientId = newOwnerId;
                m_eventBus.Publish(new EntityOwnershipTransferredEvent(entityId, oldOwnerId, newOwnerId));
            }
        }

        /// <summary>
        /// 移除指定实体的网络所有权。
        /// </summary>
        /// <param name="entityId">实体 ID。</param>
        public void RemoveOwnership(uint entityId)
        {
            TransferOwnership(entityId, -1);
        }

        /// <summary>
        /// 获取指定客户端拥有的所有实体列表。
        /// </summary>
        /// <param name="clientId">客户端 ID。</param>
        /// <returns>该客户端拥有的实体列表，可能为空。</returns>
        public List<Entity> GetOwnedEntities(int clientId)
        {
            var result = new List<Entity>();
            foreach (var entity in m_entities.Values)
            {
                if (entity.OwnerClientId == clientId)
                    result.Add(entity);
            }

            return result;
        }

        /// <summary>
        /// 当前活跃实体数量。
        /// </summary>
        public int EntityCount => m_entities.Count;
    }
}
