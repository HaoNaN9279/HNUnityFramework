#nullable enable

using MemoryPack;
using FixedMathSharp;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务配置表行定义。
    /// 由 L4 Sheet + Luban 生成，MemoryPack 可序列化。
    /// 参考 <see cref="HN.Framework.Core.Level.Logic.Inventory.ItemDef"/> 的 partial struct 模式。
    /// </summary>
    [MemoryPackable]
    public partial struct QuestDef
    {
        /// <summary>任务定义 ID。</summary>
        [MemoryPackOrder(0)]
        public int Id { get; set; }

        /// <summary>任务名称。</summary>
        [MemoryPackOrder(1)]
        public string? Name { get; set; }

        /// <summary>任务描述。</summary>
        [MemoryPackOrder(2)]
        public string? Description { get; set; }

        /// <summary>前置任务 ID 列表。全部完成后本任务才解锁。</summary>
        [MemoryPackOrder(3)]
        public List<int>? PrerequisiteQuestIds { get; set; }

        /// <summary>接受任务所需的最低等级。</summary>
        [MemoryPackOrder(4)]
        public int RequiredLevel { get; set; }

        /// <summary>时间限制（秒）。null 表示无时间限制。</summary>
        [MemoryPackOrder(5)]
        public Fixed64? TimeLimit { get; set; }

        /// <summary>条件组 ID 列表。关联 QuestConditionGroup 配置表。</summary>
        [MemoryPackOrder(6)]
        public List<int>? ConditionGroupIds { get; set; }

        /// <summary>奖励组 ID 列表。关联 QuestRewardGroup 配置表。</summary>
        [MemoryPackOrder(7)]
        public List<int>? RewardGroupIds { get; set; }

        /// <summary>是否可重复接取。</summary>
        [MemoryPackOrder(8)]
        public bool IsRepeatable { get; set; }
    }
}
