namespace HN.Framework.Core.Capability.Cutscene
{
    /// <summary>
    /// 过场动画全局配置（已废弃）。
    /// 请改用 <see cref="HN.Framework.Unity.Capability.Cutscene.CutsceneSettings"/> ScriptableObject，
    /// 通过 Project Settings > HN Unity Framework > Cutscene 面板配置。
    /// </summary>
    [System.Obsolete("Use HN.Framework.Unity.Capability.Cutscene.CutsceneSettings instead, accessible via Project Settings > HN Unity Framework > Cutscene.")]
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
