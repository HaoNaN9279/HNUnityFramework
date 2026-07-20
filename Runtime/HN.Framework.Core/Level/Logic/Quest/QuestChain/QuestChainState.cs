#nullable enable

namespace HN.Framework.Core.Level.Logic.Quest
{
    /// <summary>
    /// 任务链运行状态。
    /// </summary>
    public enum QuestChainState : byte
    {
        /// <summary>未解锁。</summary>
        Locked = 0,

        /// <summary>进行中。</summary>
        Active = 1,

        /// <summary>全部完成。</summary>
        Completed = 2,
    }
}
