using System;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 版本号配置，存储在 ScriptableObject 中。
    /// 遵循语义化版本规范（Semantic Versioning 2.0）。
    /// </summary>
    public class VersionConfig : ScriptableObject
    {
        [SerializeField]
        [Tooltip("主版本号")]
        private int m_Major = 1;

        [SerializeField]
        [Tooltip("次版本号")]
        private int m_Minor = 0;

        [SerializeField]
        [Tooltip("修订号")]
        private int m_Patch = 0;

        [SerializeField]
        [Tooltip("构建号（自动递增）")]
        private int m_BuildNumber = 0;

        [SerializeField]
        [Tooltip("预发布标签（如 alpha、beta、rc1），为空表示正式版")]
        private string m_PreReleaseTag = string.Empty;

        [SerializeField]
        [Tooltip("构建元数据标签（如 commit hash）")]
        private string m_BuildMetadata = string.Empty;

        /// <summary>
        /// 主版本号。
        /// </summary>
        public int Major
        {
            get => m_Major;
            set => m_Major = value;
        }

        /// <summary>
        /// 次版本号。
        /// </summary>
        public int Minor
        {
            get => m_Minor;
            set => m_Minor = value;
        }

        /// <summary>
        /// 修订号。
        /// </summary>
        public int Patch
        {
            get => m_Patch;
            set => m_Patch = value;
        }

        /// <summary>
        /// 构建号（自动递增）。
        /// </summary>
        public int BuildNumber
        {
            get => m_BuildNumber;
            set => m_BuildNumber = value;
        }

        /// <summary>
        /// 预发布标签。
        /// </summary>
        public string PreReleaseTag
        {
            get => m_PreReleaseTag;
            set => m_PreReleaseTag = value;
        }

        /// <summary>
        /// 构建元数据标签。
        /// </summary>
        public string BuildMetadata
        {
            get => m_BuildMetadata;
            set => m_BuildMetadata = value;
        }

        /// <summary>
        /// 获取语义化版本字符串。
        /// 格式：major.minor.patch[-preRelease][+buildMetadata]
        /// </summary>
        /// <returns>版本字符串</returns>
        public string GetVersionString()
        {
            string version = $"{m_Major}.{m_Minor}.{m_Patch}";

            if (!string.IsNullOrEmpty(m_PreReleaseTag))
            {
                version += $"-{m_PreReleaseTag}";
            }

            if (!string.IsNullOrEmpty(m_BuildMetadata))
            {
                version += $"+{m_BuildMetadata}";
            }

            return version;
        }

        /// <summary>
        /// 递增构建号。
        /// </summary>
        public void IncrementBuildNumber()
        {
            m_BuildNumber++;
        }

        /// <summary>
        /// 递增修订号，重置构建号。
        /// </summary>
        public void IncrementPatch()
        {
            m_Patch++;
            m_BuildNumber = 0;
        }

        /// <summary>
        /// 递增次版本号，重置修订号和构建号。
        /// </summary>
        public void IncrementMinor()
        {
            m_Minor++;
            m_Patch = 0;
            m_BuildNumber = 0;
        }

        /// <summary>
        /// 递增主版本号，重置次版本号、修订号和构建号。
        /// </summary>
        public void IncrementMajor()
        {
            m_Major++;
            m_Minor = 0;
            m_Patch = 0;
            m_BuildNumber = 0;
        }

        /// <summary>
        /// 获取或创建版本配置实例。
        /// </summary>
        /// <returns>版本配置实例</returns>
        public static VersionConfig GetOrCreateSettings()
        {
            string path = "Assets/Editor/BuildPipeline/Versioning/VersionConfig.asset";
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<VersionConfig>(path);
            if (config == null)
            {
                config = CreateInstance<VersionConfig>();
                UnityEditor.AssetDatabase.CreateAsset(config, path);
                UnityEditor.AssetDatabase.SaveAssets();
            }

            return config;
        }
    }
}
