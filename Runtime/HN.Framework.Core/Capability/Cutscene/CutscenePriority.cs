namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画优先级，用于 CutsceneManager 的优先级仲裁
    /// </summary>
    public enum CutscenePriority
    {
        /// <summary>背景级：空闲时播放，可被任何更高优先级中断</summary>
        Background = 0,
        /// <summary>普通级：排队等待播放</summary>
        Normal = 20,
        /// <summary>重要级：暂停低优先级过场</summary>
        Important = 50,
        /// <summary>关键级：立即停止所有过场并播放</summary>
        Critical = 100
    }
}
