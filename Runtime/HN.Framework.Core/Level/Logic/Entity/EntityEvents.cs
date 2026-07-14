namespace HN.Framework.Core.Level.Logic.Entity
{
    /// <summary>
    /// 实体生成事件。通过 <see cref="Capability.Event.EventBus"/> 发布，
    /// 供 Unity 层监听以创建对应的 GameObject。
    /// </summary>
    public readonly struct EntitySpawnedEvent
    {
        public readonly uint EntityId;
        public readonly int EntityDefId;

        public EntitySpawnedEvent(uint entityId, int entityDefId)
        {
            EntityId = entityId;
            EntityDefId = entityDefId;
        }
    }

    /// <summary>
    /// 实体销毁事件。通过 <see cref="Capability.Event.EventBus"/> 发布，
    /// 供 Unity 层监听以销毁对应的 GameObject。
    /// </summary>
    public readonly struct EntityDespawnedEvent
    {
        public readonly uint EntityId;

        public EntityDespawnedEvent(uint entityId)
        {
            EntityId = entityId;
        }
    }
}
