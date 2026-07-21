#nullable enable

using MemoryPack;
using System.Collections.Generic;
using FixedMathSharp;
using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 运行时物品实例。支持 MemoryPack 序列化和 IReference 引用池复用。
    /// 参考 Entity.cs 的 IReference + MemoryPackable 模式。
    /// </summary>
    [MemoryPackable]
    public partial class ItemInstance : IReference
    {
        [MemoryPackOrder(0)]
        public int ItemDefId { get; private set; }

        [MemoryPackOrder(1)]
        public int StackCount { get; set; }

        /// <summary>剩余耐久度。</summary>
        [MemoryPackOrder(2)]
        public Fixed64 DurabilityRemaining { get; set; }

        /// <summary>拥有该物品的 Entity ID（0 = 无拥有者）。</summary>
        [MemoryPackOrder(3)]
        public uint OwnerEntityId { get; set; }

        /// <summary>该物品在世界中的表示 Entity ID（0 = 无世界表示，如掉落物）。</summary>
        [MemoryPackOrder(4)]
        public uint WorldEntityId { get; set; }

        /// <summary>所在容器的槽位索引（-1 = 不在容器中）。</summary>
        [MemoryPackOrder(5)]
        public int SlotIndex { get; set; } = -1;

        /// <summary>该物品可装备的槽位掩码（运行时设置，完整的 ItemDef.AllowedSlots 查询由 ISheetManager 完成）。int 位掩码，每 bit 对应一个槽位。</summary>
        [MemoryPackOrder(6)]
        public int AllowedSlots { get; set; }

        /// <summary>额外数据（随机词条、附魔等）。</summary>
        [MemoryPackOrder(7)]
        public Dictionary<string, Fixed64>? ExtraData { get; set; }

        /// <summary>
        /// 初始化物品实例。由工厂方法或反序列化调用。
        /// </summary>
        internal void Initialize(int itemDefId, int stackCount = 1)
        {
            ItemDefId = itemDefId;
            StackCount = stackCount;
            DurabilityRemaining = Fixed64.Zero;
            OwnerEntityId = 0;
            WorldEntityId = 0;
            SlotIndex = -1;
            AllowedSlots = 0;
            ExtraData = null;
        }

        /// <summary>
        /// 判断是否可与另一物品实例堆叠（同类物品、未超出堆叠上限）。
        /// </summary>
        public bool CanStackWith(ItemInstance other)
        {
            if (other == null) return false;
            // 同类物品可堆叠（相同 ItemDefId、无额外数据差异）
            return ItemDefId == other.ItemDefId
                && StackCount > 0
                && other.StackCount > 0;
        }

        /// <summary>
        /// IReference 接口：重置实例，归还引用池前调用。
        /// </summary>
        public void Clear()
        {
            ItemDefId = 0;
            StackCount = 1;
            DurabilityRemaining = Fixed64.Zero;
            OwnerEntityId = 0;
            WorldEntityId = 0;
            SlotIndex = -1;
            AllowedSlots = 0;
            ExtraData = null;
        }
    }
}
