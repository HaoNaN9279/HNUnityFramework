#nullable enable

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 成就状态枚举。
    /// </summary>
    public enum AchievementState : byte
    {
        /// <summary>隐藏：需条件触发后才揭示。</summary>
        Hidden = 0,

        /// <summary>已揭示：条件追踪中。</summary>
        Revealed = 1,

        /// <summary>已完成：等待领取奖励。</summary>
        Completed = 2,

        /// <summary>已领取：奖励已发放。</summary>
        Claimed = 3
    }
}
