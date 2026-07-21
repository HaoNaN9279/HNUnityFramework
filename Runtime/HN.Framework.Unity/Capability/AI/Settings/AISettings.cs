using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HN.Framework.Unity.Capability.AI
{
    /// <summary>
    /// AI 系统全局配置 ScriptableObject。
    /// 控制 Agent 行为及决策管线的性能与精度参数。
    /// </summary>
    public class AISettings : ScriptableObject
    {
        // ── 运行时公开属性 ──

        /// <summary>
        /// Agent 每帧最大执行动作数。
        /// </summary>
        public int MaxActionsPerTick => m_MaxActionsPerTick;

        /// <summary>
        /// 模糊逻辑解模糊化采样点数。
        /// </summary>
        public int DefuzzificationSamples => m_DefuzzificationSamples;

        /// <summary>
        /// 决策管线层级数。
        /// </summary>
        public int PipelineLayerCount => m_PipelineLayerCount;

        // ── 序列化字段 ──

        [SerializeField, Range(1, 50)]
        [Tooltip("每个 Agent 每帧最大执行动作数")]
        private int m_MaxActionsPerTick = 5;

        [SerializeField, Range(10, 1000)]
        [Tooltip("模糊逻辑重心法解模糊化采样点数（值越大精度越高，计算开销越大）")]
        private int m_DefuzzificationSamples = 200;

        [SerializeField, Range(1, 10)]
        [Tooltip("决策管线层级数")]
        private int m_PipelineLayerCount = 4;

        // ── Editor 侧工厂方法 ──

#if UNITY_EDITOR
        /// <summary>
        /// 获取或创建 AI 配置资源。
        /// </summary>
        public static AISettings GetOrCreateSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetOrCreateSettings<AISettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化版本的 AI 配置资源，用于 Editor 绘制。
        /// </summary>
        public static SerializedObject GetSerializedSettings()
        {
            return Driver.Platform.HNModuleSettingsUtility
                .GetSerializedSettings<AISettings>(AssetPath);
        }

        /// <summary>
        /// 配置资源保存路径。
        /// </summary>
        public static readonly string AssetPath =
            "Assets/Project/RuntimeAssets/Core/AISettings.asset";
#endif
    }
}
