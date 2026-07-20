#nullable enable

using MemoryPack;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就配置表行定义。MemoryPack 可序列化。
    /// </summary>
    [MemoryPackable]
    public partial struct AchievementDef
    {
        /// <summary>成就定义 ID。</summary>
        [MemoryPackOrder(0)]
        public int Id { get; set; }

        /// <summary>所属分类 ID。</summary>
        [MemoryPackOrder(1)]
        public int CategoryId { get; set; }

        /// <summary>成就名称。</summary>
        [MemoryPackOrder(2)]
        public string? Name { get; set; }

        /// <summary>成就描述。</summary>
        [MemoryPackOrder(3)]
        public string? Description { get; set; }

        /// <summary>是否隐藏成就（需条件触发 Reveal）。</summary>
        [MemoryPackOrder(4)]
        public bool IsHidden { get; set; }

        /// <summary>排序权重（越小越靠前）。</summary>
        [MemoryPackOrder(5)]
        public int SortOrder { get; set; }

        /// <summary>前置成就 ID 列表。</summary>
        [MemoryPackOrder(6)]
        public List<int>? PrerequisiteAchievementIds { get; set; }

        /// <summary>条件组 ID 列表。</summary>
        [MemoryPackOrder(7)]
        public List<int>? ConditionGroupIds { get; set; }

        /// <summary>奖励组 ID 列表。</summary>
        [MemoryPackOrder(8)]
        public List<int>? RewardGroupIds { get; set; }
    }
}
