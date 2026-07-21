using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.Localization
{
    /// <summary>
    /// 本地化系统全局配置。
    /// </summary>
    public class LocalizationSettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// 默认语言代码（如 "zh-CN", "en-US", "ja-JP"）。
        /// </summary>
        public string DefaultLocale => m_DefaultLocale;

        /// <summary>
        /// 启动时自动检测系统语言。
        /// </summary>
        public bool AutoDetectLocale => m_AutoDetectLocale;

        // ── 序列化字段 ──

        [SerializeField]
        [Tooltip("默认语言代码，如 \"zh-CN\"、\"en-US\"、\"ja-JP\"")]
        private string m_DefaultLocale = "zh-CN";

        [SerializeField]
        [Tooltip("启动时自动检测系统语言（如开启则覆盖默认语言设置）")]
        private bool m_AutoDetectLocale = false;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建本地化配置资源。
        /// </summary>
        public static LocalizationSettings GetOrCreateSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<LocalizationSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的本地化配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<LocalizationSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/LocalizationSettings.asset";
#endif
    }
}
