#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Level.Logic.Combat;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 装备系统实现。通过构造函数注入槽位配置，管理 N 个装备槽位（N 由项目配置决定），
    /// 自动处理 L7 Buff 生命周期（装备时应用 Buff，卸下时移除 Buff）。
    /// <para>
    /// 槽位以 int 位掩码标识，每个槽位掩码值在容器中对应一个固定索引
    /// （由 <paramref name="slotMasks"/> 顺序决定）。槽位 ID 由项目 Luban 配置表定义。
    /// </para>
    /// </summary>
    /// <example>
    /// 项目自定义槽位配置：
    /// <code>
    /// // 假设 Luban 表定义：Weapon=bit0, Head=bit1, Chest=bit2, ...
    /// const int Weapon = 1 &lt;&lt; 0;
    /// const int Head   = 1 &lt;&lt; 1;
    /// const int Chest  = 1 &lt;&lt; 2;
    /// var slotMasks = new[] { Weapon, Head, Chest };
    /// var equipment = new Equipment(container, slotMasks, buffSystem);
    /// </code>
    /// </example>
    public sealed class Equipment : IEquipment
    {
        private readonly IContainer _container;
        private readonly IBuffSystem<uint>? _buffSystem;
        private readonly IAttributeSet<uint>? _ownerAttributes;
        private readonly IEffectPipeline<uint>? _effectPipeline;
        private readonly Dictionary<int, BuffHandle> _buffHandles = new();
        private readonly Dictionary<int, int> _slotMaskToIndex = new();
        private readonly Dictionary<int, int> _indexToSlotMask = new();

        public event Action<EquippedEvent>? OnEquipped;
        public event Action<UnequippedEvent>? OnUnequipped;
        public event Action<SlotSwapEvent>? OnSlotSwap;
        public event Action<AllUnequippedEvent>? OnAllUnequipped;
        public event Action<FailedToEquipEvent>? OnFailedToEquip;

        /// <summary>
        /// 初始化装备系统。
        /// </summary>
        /// <param name="container">装备栏容器。容量须 &gt;= <paramref name="slotMasks"/> 长度。</param>
        /// <param name="slotMasks">槽位配置列表，列表索引 = 容器槽位索引，值为该槽位的位掩码。
        /// 值为位掩码（如 1 &lt;&lt; 0 表示 bit 0 的槽位），由项目 Luban 配置表或自定义常量定义。</param>
        /// <param name="buffSystem">L7 Buff 系统。为 null 时不启用 Buff 自动生命周期。</param>
        /// <param name="ownerAttributes">拥有者属性集。为 null 时不操作属性。</param>
        /// <param name="effectPipeline">L7 效果管线。为 null 时不启用 Buff 自动生命周期。</param>
        /// <exception cref="ArgumentNullException">container 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">slotMasks 为 null 或为空时抛出。</exception>
        public Equipment(
            IContainer container,
            IReadOnlyList<int> slotMasks,
            IBuffSystem<uint>? buffSystem = null,
            IAttributeSet<uint>? ownerAttributes = null,
            IEffectPipeline<uint>? effectPipeline = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _buffSystem = buffSystem;
            _ownerAttributes = ownerAttributes;
            _effectPipeline = effectPipeline;

            if (slotMasks == null || slotMasks.Count == 0)
                throw new ArgumentException("槽位配置不能为空", nameof(slotMasks));

            if (slotMasks.Count > container.Capacity)
                throw new ArgumentException(
                    $"槽位数量 ({slotMasks.Count}) 超过容器容量 ({container.Capacity})", nameof(slotMasks));

            // 建立 slotMask ↔ containerIndex 双向映射
            for (int i = 0; i < slotMasks.Count; i++)
            {
                int mask = slotMasks[i];
                if (mask == 0)
                    throw new ArgumentException($"槽位索引 {i} 的掩码不能为 0", nameof(slotMasks));

                _slotMaskToIndex[mask] = i;
                _indexToSlotMask[i] = mask;
            }
        }

        /// <inheritdoc />
        public bool CanEquip(ItemInstance item, int slotMask)
        {
            if (item == null) return false;
            if (slotMask == 0) return false;

            // 检查槽位是否存在于当前配置中
            if (!_slotMaskToIndex.ContainsKey(slotMask)) return false;

            // 检查物品的 AllowedSlots 位掩码是否包含目标槽位
            if (item.ItemDefId <= 0) return false;
            return (item.AllowedSlots & slotMask) == slotMask;
        }

        /// <inheritdoc />
        public bool Equip(int slotMask, ItemInstance item)
        {
            if (!CanEquip(item, slotMask))
            {
                OnFailedToEquip?.Invoke(new FailedToEquipEvent(slotMask, item.ItemDefId, "槽位不匹配"));
                return false;
            }

            if (!_slotMaskToIndex.TryGetValue(slotMask, out int index))
            {
                OnFailedToEquip?.Invoke(new FailedToEquipEvent(slotMask, item.ItemDefId, "无效槽位"));
                return false;
            }

            // 若槽位已有装备，先卸下
            var currentSlot = _container.GetSlot(index);
            if (!currentSlot.IsEmpty)
            {
                Unequip(slotMask);
            }

            // 放置物品到容器槽位
            if (!_container.TryAddItemAt(index, item))
            {
                OnFailedToEquip?.Invoke(new FailedToEquipEvent(slotMask, item.ItemDefId, "容器操作失败"));
                return false;
            }

            // 应用 Buff
            ApplyItemBuffs(item, slotMask);

            OnEquipped?.Invoke(new EquippedEvent(slotMask, item));
            return true;
        }

        /// <inheritdoc />
        public ItemInstance? Unequip(int slotMask)
        {
            if (!_slotMaskToIndex.TryGetValue(slotMask, out int index)) return null;

            var slotData = _container.GetSlot(index);
            if (slotData.IsEmpty) return null;

            var item = slotData.Item;

            // 移除 Buff
            RemoveItemBuffs(slotMask);

            // 从容器移除
            _container.RemoveItemAt(index);

            OnUnequipped?.Invoke(new UnequippedEvent(slotMask, item!.ItemDefId));
            return item;
        }

        /// <inheritdoc />
        public bool SwapSlots(int slotMaskA, int slotMaskB)
        {
            if (slotMaskA == slotMaskB) return false;
            if (!_slotMaskToIndex.TryGetValue(slotMaskA, out int indexA)) return false;
            if (!_slotMaskToIndex.TryGetValue(slotMaskB, out int indexB)) return false;

            // 先移除两边 Buff
            RemoveItemBuffs(slotMaskA);
            RemoveItemBuffs(slotMaskB);

            // 交换容器槽位
            _container.SwapSlots(indexA, indexB);

            // 重新应用 Buff
            var slotDataA = _container.GetSlot(indexA);
            var slotDataB = _container.GetSlot(indexB);

            if (!slotDataA.IsEmpty && slotDataA.Item != null)
            {
                ApplyItemBuffs(slotDataA.Item, slotMaskA);
            }

            if (!slotDataB.IsEmpty && slotDataB.Item != null)
            {
                ApplyItemBuffs(slotDataB.Item, slotMaskB);
            }

            OnSlotSwap?.Invoke(new SlotSwapEvent(slotMaskA, slotMaskB));
            return true;
        }

        /// <inheritdoc />
        public ItemInstance? GetEquippedItem(int slotMask)
        {
            if (!_slotMaskToIndex.TryGetValue(slotMask, out int index)) return null;
            var slotData = _container.GetSlot(index);
            return slotData.Item;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, ItemInstance> GetAllEquipped()
        {
            var result = new Dictionary<int, ItemInstance>();
            foreach (var (slotMask, index) in _slotMaskToIndex)
            {
                var slotData = _container.GetSlot(index);
                if (!slotData.IsEmpty && slotData.Item != null)
                {
                    result[slotMask] = slotData.Item;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public int GetEquippableSlots(ItemInstance item)
        {
            if (item == null) return 0;
            if (item.ItemDefId <= 0) return 0;

            // 返回物品 AllowedSlots 与当前装备系统已知槽位的交集
            int result = 0;
            foreach (var (slotMask, _) in _slotMaskToIndex)
            {
                if ((item.AllowedSlots & slotMask) == slotMask)
                {
                    result |= slotMask;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void UnequipAll()
        {
            foreach (var (slotMask, index) in _slotMaskToIndex)
            {
                var slotData = _container.GetSlot(index);
                if (!slotData.IsEmpty)
                {
                    RemoveItemBuffs(slotMask);
                    _container.RemoveItemAt(index);
                }
            }

            OnAllUnequipped?.Invoke(default);
        }

        // ==================== 私有辅助方法 ====================

        /// <summary>
        /// 应用物品 Buff 到拥有者。
        /// </summary>
        private void ApplyItemBuffs(ItemInstance item, int slotMask)
        {
            if (_buffSystem == null || _ownerAttributes == null || _effectPipeline == null) return;

            // TODO: 从 ISheetManager 获取 ItemDef，遍历 ItemDef.Buffs
            // 当前骨架实现：
            // ItemDef def = _itemDefResolver(item.ItemDefId);
            // if (def.Buffs != null)
            // {
            //     foreach (var itemBuffSpec in def.Buffs)
            //     {
            //         var l7Spec = itemBuffSpec.BuildBuffSpec<uint>(item.OwnerEntityId);
            //         var handle = _buffSystem.ApplyBuff(l7Spec, item.OwnerEntityId, item.OwnerEntityId, _ownerAttributes, _effectPipeline);
            //         _buffHandles[slotMask] = handle;
            //     }
            // }
        }

        /// <summary>
        /// 移除槽位对应的 Buff。
        /// </summary>
        private void RemoveItemBuffs(int slotMask)
        {
            if (_buffSystem == null || _ownerAttributes == null || _effectPipeline == null) return;

            if (_buffHandles.TryGetValue(slotMask, out var handle) && handle.IsValid)
            {
                _buffSystem.RemoveBuff(handle, _ownerAttributes, _effectPipeline);
                _buffHandles.Remove(slotMask);
            }
        }
    }
}
