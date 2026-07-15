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

    /// <summary>
    /// 实体所有权转移事件。当实体的 OwnerClientId 发生变化时，
    /// 通过 <see cref="Capability.Event.EventBus"/> 发布，
    /// 供 Unity 层监听以更新 FishNet NetworkObject 的所有权。
    /// </summary>
    public readonly struct EntityOwnershipTransferredEvent
    {
        public readonly uint EntityId;
        public readonly int OldOwnerId;
        public readonly int NewOwnerId;

        public EntityOwnershipTransferredEvent(uint entityId, int oldOwnerId, int newOwnerId)
        {
            EntityId = entityId;
            OldOwnerId = oldOwnerId;
            NewOwnerId = newOwnerId;
        }
    }
}