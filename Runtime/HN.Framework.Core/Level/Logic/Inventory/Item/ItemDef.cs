#nullable enable

using MemoryPack;
using FixedMathSharp;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 物品配置表行定义。
    /// 由 L4 Sheet + Luban 生成，MemoryPack 可序列化。
    /// </summary>
    [MemoryPackable]
    public partial struct ItemDef
    {
        [MemoryPackOrder(0)]
        public int Id { get; set; }

        [MemoryPackOrder(1)]
        public string Name { get; set; }

        /// <summary>物品类别 ID，由项目 Luban 配置表定义。</summary>
        [MemoryPackOrder(2)]
        public int Category { get; set; }

        [MemoryPackOrder(3)]
        public string Description { get; set; }

        /// <summary>最大堆叠数（默认 1，不可堆叠）。</summary>
        [MemoryPackOrder(4)]
        public int MaxStack { get; set; }

        /// <summary>允许装备的槽位（int 位掩码），0 表示不可装备。每 bit 对应一个由项目 Luban 配置表定义的槽位 ID。</summary>
        [MemoryPackOrder(5)]
        public int AllowedSlots { get; set; }

        /// <summary>耐久度上限（0 = 无限耐久）。</summary>
        [MemoryPackOrder(6)]
        public Fixed64 Durability { get; set; }

        /// <summary>物品重量。</summary>
        [MemoryPackOrder(7)]
        public Fixed64 Weight { get; set; }

        /// <summary>出售价格。</summary>
        [MemoryPackOrder(8)]
        public Fixed64 SellPrice { get; set; }

        /// <summary>购买价格。</summary>
        [MemoryPackOrder(9)]
        public Fixed64 BuyPrice { get; set; }

        /// <summary>物品图标 Addressables Label。</summary>
        [MemoryPackOrder(10)]
        public string? IconLabel { get; set; }

        /// <summary>世界预制体（掉落物/展示）Addressables Label。</summary>
        [MemoryPackOrder(11)]
        public string? WorldPrefabLabel { get; set; }

        /// <summary>品质等级。</summary>
        [MemoryPackOrder(12)]
        public int Quality { get; set; }

        /// <summary>物品附带的 Buff 规格列表。</summary>
        [MemoryPackOrder(13)]
        public List<ItemBuffSpec>? Buffs { get; set; }
    }
}
