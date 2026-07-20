#nullable enable

using System.Collections.Generic;
using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 组合条件运算符。
    /// </summary>
    public enum CompositeOperator : byte
    {
        /// <summary>全部子条件满足时，组合条件满足。</summary>
        And = 0,

        /// <summary>任意子条件满足时，组合条件满足。</summary>
        Or = 1,

        /// <summary>子条件不满足时，组合条件满足（取反，仅接受单个子条件）。</summary>
        Not = 2,
    }

    /// <summary>
    /// 组合条件定义。
    /// 将多个子条件通过逻辑运算符组合为复合条件。
    /// </summary>
    public sealed class CompositeCondition
    {
        /// <summary>组合条件唯一标识。</summary>
        public int Id { get; set; }

        /// <summary>子条件组合运算符。</summary>
        public CompositeOperator Operator { get; set; } = CompositeOperator.And;

        /// <summary>子条件 ID 列表。</summary>
        public List<int> SubConditionIds { get; set; } = new();
    }

    /// <summary>
    /// 条件组配置表行定义。
    /// 组内条件全部为 AND 关系；多个条件组之间为互斥 OR 关系。
    /// 由 L4 Sheet + Luban 生成，MemoryPack 可序列化。
    /// </summary>
    [MemoryPackable]
    public partial struct ConditionGroupDef
    {
        /// <summary>条件组唯一标识。</summary>
        [MemoryPackOrder(0)]
        public int Id;

        /// <summary>
        /// 条件组内的条件 ID 列表。
        /// 组内所有条件必须全部满足（AND 关系）。
        /// </summary>
        [MemoryPackOrder(1)]
        public List<int>? ConditionIds;

        /// <summary>条件组描述（调试用）。</summary>
        [MemoryPackOrder(2)]
        public string? Description;
    }
}
