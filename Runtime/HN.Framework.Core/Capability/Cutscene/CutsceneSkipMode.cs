namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 跳过模式
    /// </summary>
    public enum CutsceneSkipMode
    {
        /// <summary>不允许跳过</summary>
        None = 0,
        /// <summary>立即跳过</summary>
        Immediate,
        /// <summary>当前片段结束后跳过</summary>
        EndOfSegment
    }
}
