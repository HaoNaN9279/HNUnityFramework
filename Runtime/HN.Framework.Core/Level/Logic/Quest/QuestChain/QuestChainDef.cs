#nullable enable

using MemoryPack;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务链配置表行定义。一条 QuestChain 由一组按顺序推进的任务组成，
    /// 支持基于条件的分支选择。
    /// </summary>
    [MemoryPackable]
    public partial struct QuestChainDef
    {
        /// <summary>唯一标识。</summary>
        [MemoryPackOrder(0)]
        public int Id;

        /// <summary>显示名称。</summary>
        [MemoryPackOrder(1)]
        public string? Name;

        /// <summary>描述文本。</summary>
        [MemoryPackOrder(2)]
        public string? Description;

        /// <summary>
        /// 有序任务 ID 列表。列表顺序决定任务链的线性推进顺序，
        /// 索引 0 为起始任务。
        /// </summary>
        [MemoryPackOrder(3)]
        public List<int> QuestIds;

        /// <summary>
        /// 分支条件 ID 列表，与 <see cref="QuestIds"/> 一一对应。
        /// 每个元素对应 CompositeCondition 的 Id，用于决定该索引位置是否选用对应任务。
        /// 为 <c>null</c> 表示无分支逻辑，按 <see cref="QuestIds"/> 顺序线性推进。
        /// </summary>
        [MemoryPackOrder(4)]
        public List<int>? BranchConditionIds;

        /// <summary>
        /// 默认分支索引。当所有 <see cref="BranchConditionIds"/> 均不满足时，
        /// 使用此索引对应的任务作为推进目标。默认值为 0。
        /// </summary>
        [MemoryPackOrder(5)]
        public int DefaultBranchIndex;
    }
}
