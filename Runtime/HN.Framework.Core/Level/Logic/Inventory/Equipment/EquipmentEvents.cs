#nullable enable

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>装备事件。</summary>
    public readonly struct EquippedEvent
    {
        public readonly int SlotIndex;
        public readonly ItemInstance Item;

        public EquippedEvent(int slotIndex, ItemInstance item)
        {
            SlotIndex = slotIndex;
            Item = item;
        }
    }

    /// <summary>卸下装备事件。</summary>
    public readonly struct UnequippedEvent
    {
        public readonly int SlotIndex;
        public readonly int ItemDefId;

        public UnequippedEvent(int slotIndex, int itemDefId)
        {
            SlotIndex = slotIndex;
            ItemDefId = itemDefId;
        }
    }

    /// <summary>槽位交换事件。</summary>
    public readonly struct SlotSwapEvent
    {
        public readonly int SourceSlot;
        public readonly int TargetSlot;

        public SlotSwapEvent(int sourceSlot, int targetSlot)
        {
            SourceSlot = sourceSlot;
            TargetSlot = targetSlot;
        }
    }

    /// <summary>全部卸下事件。</summary>
    public readonly struct AllUnequippedEvent
    {
        // 无额外字段，表示所有槽位被清空
    }

    /// <summary>装备失败事件。</summary>
    public readonly struct FailedToEquipEvent
    {
        public readonly int SlotIndex;
        public readonly int ItemDefId;
        public readonly string Reason;

        public FailedToEquipEvent(int slotIndex, int itemDefId, string reason)
        {
            SlotIndex = slotIndex;
            ItemDefId = itemDefId;
            Reason = reason;
        }
    }
}
