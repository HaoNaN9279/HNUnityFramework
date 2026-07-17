#nullable enable

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>物品添加事件。</summary>
    public readonly struct ItemAddedEvent
    {
        /// <summary>所在容器的索引。</summary>
        public readonly int ContainerIndex;
        /// <summary>槽位索引。</summary>
        public readonly int SlotIndex;
        /// <summary>添加的物品实例。</summary>
        public readonly ItemInstance Item;

        public ItemAddedEvent(int containerIndex, int slotIndex, ItemInstance item)
        {
            ContainerIndex = containerIndex;
            SlotIndex = slotIndex;
            Item = item;
        }
    }

    /// <summary>物品移除事件。</summary>
    public readonly struct ItemRemovedEvent
    {
        public readonly int ContainerIndex;
        public readonly int SlotIndex;
        public readonly int ItemDefId;
        public readonly int RemovedCount;

        public ItemRemovedEvent(int containerIndex, int slotIndex, int itemDefId, int removedCount)
        {
            ContainerIndex = containerIndex;
            SlotIndex = slotIndex;
            ItemDefId = itemDefId;
            RemovedCount = removedCount;
        }
    }

    /// <summary>堆叠数变更事件。</summary>
    public readonly struct ItemStackChangedEvent
    {
        public readonly int ContainerIndex;
        public readonly int SlotIndex;
        public readonly int OldCount;
        public readonly int NewCount;

        public ItemStackChangedEvent(int containerIndex, int slotIndex, int oldCount, int newCount)
        {
            ContainerIndex = containerIndex;
            SlotIndex = slotIndex;
            OldCount = oldCount;
            NewCount = newCount;
        }
    }

    /// <summary>物品移动事件（交换槽位）。</summary>
    public readonly struct ItemMovedEvent
    {
        public readonly int ContainerIndex;
        public readonly int SourceSlotIndex;
        public readonly int TargetSlotIndex;

        public ItemMovedEvent(int containerIndex, int sourceSlot, int targetSlot)
        {
            ContainerIndex = containerIndex;
            SourceSlotIndex = sourceSlot;
            TargetSlotIndex = targetSlot;
        }
    }

    /// <summary>容器被清空事件。</summary>
    public readonly struct ContainerClearedEvent
    {
        public readonly int ContainerIndex;

        public ContainerClearedEvent(int containerIndex)
        {
            ContainerIndex = containerIndex;
        }
    }
}
