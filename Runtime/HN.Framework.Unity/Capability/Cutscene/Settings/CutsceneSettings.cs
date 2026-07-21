using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.Cutscene
{
    /// <summary>
    /// 过场动画系统全局配置 ScriptableObject。
    /// 替代原有的 <see cref="HN.Framework.Core.Capability.Cutscene.CutsceneGlobalSettings"/> struct 默认值。
    /// </summary>
    public class CutsceneSettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// 是否全局允许跳过过场动画。
        /// </summary>
        public bool GlobalSkipEnabled => m_GlobalSkipEnabled;

        /// <summary>
        /// 全局播放速度倍率（1.0 = 正常速度）。
        /// </summary>
        public float GlobalSpeedMultiplier => m_GlobalSpeedMultiplier;

        // ── 序列化字段 ──

        [SerializeField]
        [Tooltip("是否全局允许跳过过场动画")]
        private bool m_GlobalSkipEnabled = true;

        [SerializeField, Range(0.1f, 5f)]
        [Tooltip("全局播放速度倍率（1.0 = 正常速度）")]
        private float m_GlobalSpeedMultiplier = 1f;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建过场配置资源。
        /// </summary>
        public static CutsceneSettings GetOrCreateSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<CutsceneSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的过场配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<CutsceneSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/CutsceneSettings.asset";
#endif
    }
}
