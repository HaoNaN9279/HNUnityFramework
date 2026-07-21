using UnityEngine;
using HN.Framework.Unity.Driver.Platform;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建管线全局设置，存储在 ScriptableObject 中。
    /// </summary>
    public class BuildPipelineSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("构建输出根目录（相对于项目根）")]
        private string m_BuildOutputRoot = "Builds";

        [SerializeField]
        [Tooltip("是否在构建前启用资源质检门禁")]
        private bool m_EnablePreBuildValidation = true;

        [SerializeField]
        [Tooltip("质检未通过时是否阻止构建")]
        private bool m_BlockBuildOnValidationFailure = true;

        [SerializeField]
        [Tooltip("是否自动生成构建清单")]
        private bool m_AutoGenerateBuildManifest = true;

        [SerializeField]
        [Tooltip("版本配置资源路径")]
        private string m_VersionConfigPath = "Assets/Editor/BuildPipeline/Versioning/VersionConfig.asset";

        /// <summary>
        /// 构建输出根目录（相对于项目根）。
        /// </summary>
        public string BuildOutputRoot => m_BuildOutputRoot;

        /// <summary>
        /// 是否在构建前启用资源质检门禁。
        /// </summary>
        public bool EnablePreBuildValidation
        {
            get => m_EnablePreBuildValidation;
            set => m_EnablePreBuildValidation = value;
        }

        /// <summary>
        /// 质检未通过时是否阻止构建。
        /// </summary>
        public bool BlockBuildOnValidationFailure
        {
            get => m_BlockBuildOnValidationFailure;
            set => m_BlockBuildOnValidationFailure = value;
        }

        /// <summary>
        /// 是否自动生成构建清单。
        /// </summary>
        public bool AutoGenerateBuildManifest
        {
            get => m_AutoGenerateBuildManifest;
            set => m_AutoGenerateBuildManifest = value;
        }

        /// <summary>
        /// 版本配置资源路径。
        /// </summary>
        public string VersionConfigPath => m_VersionConfigPath;

        private static readonly string AssetPath =
            "Assets/Editor/BuildPipeline/Settings/BuildPipelineSettings.asset";

        /// <summary>
        /// 获取或创建设置实例。
        /// </summary>
        /// <returns>设置实例</returns>
        public static BuildPipelineSettings GetOrCreateSettings()
        {
            return HNModuleSettingsUtility.GetOrCreateSettings<BuildPipelineSettings>(AssetPath);
        }

        /// <summary>
        /// 获取序列化设置对象。
        /// </summary>
        /// <returns>序列化对象</returns>
        public static UnityEditor.SerializedObject GetSerializedSettings()
        {
            return HNModuleSettingsUtility.GetSerializedSettings<BuildPipelineSettings>(AssetPath);
        }
    }
}
