namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画播放状态枚举
    /// </summary>
    public enum CutsceneState
    {
        /// <summary>空闲/未播放</summary>
        Idle = 0,
        /// <summary>播放中</summary>
        Playing,
        /// <summary>暂停</summary>
        Paused,
        /// <summary>停止中（过渡状态）</summary>
        Stopping,
        /// <summary>已完成</summary>
        Finished,
        /// <summary>已跳过</summary>
        Skipped
    }
}
