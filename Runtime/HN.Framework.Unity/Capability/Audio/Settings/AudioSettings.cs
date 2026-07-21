using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.Audio
{
    /// <summary>
    /// 音频系统全局配置。
    /// </summary>
    public class AudioSettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// 主音量（0-1）。
        /// </summary>
        public float MasterVolume => m_MasterVolume;

        /// <summary>
        /// 最大并发音效数。
        /// </summary>
        public int MaxConcurrentSounds => m_MaxConcurrentSounds;

        /// <summary>
        /// 启用空间音频。
        /// </summary>
        public bool EnableSpatialAudio => m_EnableSpatialAudio;

        // ── 序列化字段 ──

        [SerializeField, Range(0f, 1f)]
        [Tooltip("主音量")]
        private float m_MasterVolume = 1.0f;

        [SerializeField, Range(1, 256)]
        [Tooltip("最大并发音效数")]
        private int m_MaxConcurrentSounds = 64;

        [SerializeField]
        [Tooltip("启用空间音频（3D 定位）")]
        private bool m_EnableSpatialAudio = false;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建音频配置资源。
        /// </summary>
        public static AudioSettings GetOrCreateSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<AudioSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的音频配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<AudioSettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/AudioSettings.asset";
#endif
    }
}
