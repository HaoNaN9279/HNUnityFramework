using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.Asset
{
    /// <summary>
    /// 资源系统全局配置 ScriptableObject。
    /// 控制自动卸载策略等运行时行为参数。
    /// </summary>
    public class AssetSettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// 资源自动卸载延迟（秒）。引用计数归零后超过此时间自动回收资源。
        /// </summary>
        public float AutoUnloadDelay => m_AutoUnloadDelay;

        /// <summary>
        /// 是否启用资源自动卸载。
        /// </summary>
        public bool EnableAutoUnload => m_EnableAutoUnload;

        // ── 序列化字段 ──

        [SerializeField, Range(1f, 300f)]
        [Tooltip("资源自动卸载延迟（秒），引用计数归零后超过此时间自动回收")]
        private float m_AutoUnloadDelay = 30f;

        [SerializeField]
        [Tooltip("是否启用资源自动卸载")]
        private bool m_EnableAutoUnload = true;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建资源配置。
        /// </summary>
        public static AssetSettings GetOrCreateSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<AssetSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的资源配置，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<AssetSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/AssetSettings.asset";
#endif
    }
}
