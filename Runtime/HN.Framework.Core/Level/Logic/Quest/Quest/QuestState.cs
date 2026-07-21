#nullable enable

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务状态枚举。
    /// </summary>
    public enum QuestState : byte
    {
        /// <summary>未解锁（前置任务未完成/等级不够）。</summary>
        Locked = 0,

        /// <summary>进行中。</summary>
        Active = 1,

        /// <summary>条件已满足，等待领取奖励。</summary>
        Completed = 2,

        /// <summary>已领取奖励。</summary>
        Claimed = 3,

        /// <summary>已失败（超时或外部触发）。</summary>
        Failed = 4,
    }
}
