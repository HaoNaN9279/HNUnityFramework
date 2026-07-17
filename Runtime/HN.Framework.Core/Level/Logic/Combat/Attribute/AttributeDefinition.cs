#nullable enable

using FixedMathSharp;
using MemoryPack;
using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Combat
{
    /// <summary>
    /// 属性类型定义表行。由 Luban 从配置表生成，通过 L4 Sheet 运行时加载。
    /// 每个属性类型对应一行定义，包含唯一名称和初始值。
    /// </summary>
    [MemoryPackable]
    public partial class AttributeDefinition
    {
        /// <summary>属性类型 ID，与 <see cref="AttributeType._index"/> 对应。</summary>
        [MemoryPackOrder(0)]
        public int Id { get; set; }

        /// <summary>属性名称，如 "MaxHP"、"ATK"、"DEF"。</summary>
        [MemoryPackOrder(1)]
        public string Name { get; set; } = string.Empty;

        /// <summary>属性初始值（BaseValue）。</summary>
        [MemoryPackOrder(2)]
        public Fixed64 InitialValue { get; set; }

        /// <summary>最小值下限（可选，-1 表示无下限）。</summary>
        [MemoryPackOrder(3)]
        public Fixed64 MinValue { get; set; } = Fixed64.MinValue;

        /// <summary>最大值上限（可选，-1 表示无上限）。</summary>
        [MemoryPackOrder(4)]
        public Fixed64 MaxValue { get; set; } = Fixed64.MaxValue;

        /// <summary>描述。</summary>
        [MemoryPackOrder(5)]
        public string Description { get; set; } = string.Empty;
    }
}
