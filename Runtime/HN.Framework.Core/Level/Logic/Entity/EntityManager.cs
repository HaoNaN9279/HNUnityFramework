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
            var entity = ReferencePool.Acquire<Entity>();
            uint id = m_nextEntityId++;
            entity.Initialize(id, entityDefId);
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
        /// 当前活跃实体数量。
        /// </summary>
        public int EntityCount => m_entities.Count;
    }
}
