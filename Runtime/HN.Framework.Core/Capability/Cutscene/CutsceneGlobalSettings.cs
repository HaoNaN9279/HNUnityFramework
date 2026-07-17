namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画全局配置
    /// </summary>
    public struct CutsceneGlobalSettings
    {
        /// <summary>是否全局允许跳过过场</summary>
        public bool GlobalSkipEnabled { get; set; }

        /// <summary>全局播放速度倍率（1.0 = 正常速度）</summary>
        public float GlobalSpeedMultiplier { get; set; }

        /// <summary>默认全局设置</summary>
        public static CutsceneGlobalSettings Default => new CutsceneGlobalSettings
        {
            GlobalSkipEnabled = true,
            GlobalSpeedMultiplier = 1f
        };
    }
}
