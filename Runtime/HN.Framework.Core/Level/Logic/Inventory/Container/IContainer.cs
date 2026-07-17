#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 容器接口。背包、仓库、装备栏等通用。
    /// </summary>
    public interface IContainer
    {
        /// <summary>容器类型。</summary>
        ContainerType Type { get; }

        /// <summary>容器总容量（槽位数）。</summary>
        int Capacity { get; }

        /// <summary>当前已使用的槽位数。</summary>
        int SlotCount { get; }

        /// <summary>获取指定槽位数据。</summary>
        ContainerSlot GetSlot(int index);

        /// <summary>尝试添加物品（自动寻找空槽位或堆叠）。</summary>
        bool TryAddItem(ItemInstance item);

        /// <summary>尝试添加到指定槽位。</summary>
        bool TryAddItemAt(int index, ItemInstance item);

        /// <summary>从指定槽位移除 count 个物品。</summary>
        bool RemoveItem(int index, int count);

        /// <summary>完全移除指定槽位中的物品。</summary>
        bool RemoveItemAt(int index);

        /// <summary>交换两个槽位的物品。</summary>
        void SwapSlots(int indexA, int indexB);

        /// <summary>清空容器。</summary>
        void Clear();

        /// <summary>获取容器中某类物品的总数量。</summary>
        int GetItemCount(int itemDefId);

        /// <summary>判断容器中是否有某类物品达到指定数量。</summary>
        bool HasItem(int itemDefId, int count = 1);

        // ---------- 事件 ----------
        event Action<ItemAddedEvent>? OnItemAdded;
        event Action<ItemRemovedEvent>? OnItemRemoved;
        event Action<ItemStackChangedEvent>? OnStackChanged;
        event Action<ItemMovedEvent>? OnItemMoved;
        event Action<ContainerClearedEvent>? OnCleared;
    }
}
