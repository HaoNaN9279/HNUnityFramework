using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// UI 系统全局配置 ScriptableObject。
    /// 控制 Toast、动画、模态遮罩等运行时行为参数。
    /// </summary>
    public class UISettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// Toast 默认显示时长（秒）。
        /// </summary>
        public float DefaultToastDuration => m_DefaultToastDuration;

        /// <summary>
        /// 最大并发 Toast 数量。
        /// </summary>
        public int MaxConcurrentToasts => m_MaxConcurrentToasts;

        /// <summary>
        /// Toast 入场/退场动画时长（秒）。
        /// </summary>
        public float ToastAnimationDuration => m_ToastAnimationDuration;

        /// <summary>
        /// 通用 UI 动画默认时长（秒），用于 FadeIn/FadeOut/SlideIn/SlideOut/ScaleIn/ScaleOut。
        /// </summary>
        public float DefaultAnimationDuration => m_DefaultAnimationDuration;

        /// <summary>
        /// 模态弹窗遮罩透明度（0-1）。
        /// </summary>
        public float DialogMaskAlpha => m_DialogMaskAlpha;

        /// <summary>
        /// 程序化创建 Toast 时的默认字号。
        /// </summary>
        public int ToastFontSize => m_ToastFontSize;

        // ── 序列化字段 ──

        [SerializeField, Range(0.5f, 10f)]
        [Tooltip("Toast 默认显示时长（秒）")]
        private float m_DefaultToastDuration = 2f;

        [SerializeField, Range(1, 10)]
        [Tooltip("最大并发 Toast 数量")]
        private int m_MaxConcurrentToasts = 3;

        [SerializeField, Range(0.05f, 1f)]
        [Tooltip("Toast 入场/退场动画时长（秒）")]
        private float m_ToastAnimationDuration = 0.2f;

        [SerializeField, Range(0.1f, 2f)]
        [Tooltip("通用 UI 动画默认时长（秒）")]
        private float m_DefaultAnimationDuration = 0.3f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("模态弹窗遮罩透明度")]
        private float m_DialogMaskAlpha = 0.5f;

        [SerializeField, Range(8, 72)]
        [Tooltip("程序化 Toast 默认字号")]
        private int m_ToastFontSize = 24;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建 UI 配置资源。
        /// </summary>
        public static UISettings GetOrCreateSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<UISettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的 UI 配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return HN.Framework.Unity.Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<UISettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/UISettings.asset";
#endif
    }
}
