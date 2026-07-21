#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 装备系统接口。管理装备槽位的装卸与 Buff 生命周期联动。
    /// 槽位以 int 位掩码标识，槽位 ID 由项目 Luban 配置表定义，每 bit 对应一个槽位。
    /// </summary>
    public interface IEquipment
    {
        /// <summary>
        /// 检查指定物品是否能装备到指定槽位。
        /// </summary>
        /// <param name="item">待装备物品。</param>
        /// <param name="slotMask">目标装备槽位掩码。</param>
        /// <returns>true 表示可装备。</returns>
        bool CanEquip(ItemInstance item, int slotMask);

        /// <summary>
        /// 装备物品到指定槽位。若槽位已有装备则自动卸下旧装备。
        /// </summary>
        /// <param name="slotMask">目标装备槽位掩码。</param>
        /// <param name="item">待装备物品。</param>
        /// <returns>true 表示装备成功。</returns>
        bool Equip(int slotMask, ItemInstance item);

        /// <summary>
        /// 从指定槽位卸下装备。
        /// </summary>
        /// <param name="slotMask">目标装备槽位掩码。</param>
        /// <returns>卸下的物品，若槽位为空则返回 null。</returns>
        ItemInstance? Unequip(int slotMask);

        /// <summary>
        /// 交换两个槽位的装备。
        /// </summary>
        /// <param name="slotMaskA">槽位 A 掩码。</param>
        /// <param name="slotMaskB">槽位 B 掩码。</param>
        /// <returns>true 表示交换成功。</returns>
        bool SwapSlots(int slotMaskA, int slotMaskB);

        /// <summary>
        /// 获取指定槽位的装备物品。
        /// </summary>
        /// <param name="slotMask">目标装备槽位掩码。</param>
        /// <returns>装备的物品，若槽位为空则返回 null。</returns>
        ItemInstance? GetEquippedItem(int slotMask);

        /// <summary>
        /// 获取所有已装备的物品。
        /// </summary>
        /// <returns>槽位掩码到物品的映射。</returns>
        IReadOnlyDictionary<int, ItemInstance> GetAllEquipped();

        /// <summary>
        /// 获取指定物品可装备的槽位掩码。
        /// </summary>
        /// <param name="item">物品实例。</param>
        /// <returns>允许装备的槽位位掩码组合（0 表示不可装备）。</returns>
        int GetEquippableSlots(ItemInstance item);

        /// <summary>
        /// 清空所有装备槽。
        /// </summary>
        void UnequipAll();

        // ---------- 事件 ----------

        /// <summary>装备成功事件。</summary>
        event Action<EquippedEvent>? OnEquipped;

        /// <summary>卸下装备事件。</summary>
        event Action<UnequippedEvent>? OnUnequipped;

        /// <summary>槽位交换事件。</summary>
        event Action<SlotSwapEvent>? OnSlotSwap;

        /// <summary>全部卸下事件。</summary>
        event Action<AllUnequippedEvent>? OnAllUnequipped;

        /// <summary>装备失败事件。</summary>
        event Action<FailedToEquipEvent>? OnFailedToEquip;
    }
}
