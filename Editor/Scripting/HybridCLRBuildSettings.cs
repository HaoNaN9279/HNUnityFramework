using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR 构建配置（已废弃）。
    /// 请改用 <see cref="HybridCLRSettings"/>，通过 Project Settings > HN Unity Framework > HybridCLR 面板配置。
    /// </summary>
    [System.Obsolete("Use HybridCLRSettings instead, accessible via Project Settings > HN Unity Framework > HybridCLR.")]
    public class HybridCLRBuildSettings : ScriptableObject
    {
        [Header("AOT Metadata")]
        public string aotAssembliesRoot = "Assets/StreamingAssets/AOTMetadata";

        public string[] aotAssemblyNames = new[] { "Assembly-CSharp", "Assembly-CSharp-firstpass" };

        [Header("Hot Update Assemblies")]
        public string hotUpdateAssembliesRoot = "Assets/StreamingAssets/HotUpdates";

        public string[] hotUpdateAssemblyNames = new[] { "HotFix" };

        [Header("Build Options")]
        public bool autoGenerateMetadata = true;

        public bool autoCopyNativeLibs = true;

        /// <summary>
        /// 在 Assets 菜单中创建 HybridCLRBuildSettings 资产。
        /// </summary>
        [MenuItem("Assets/Create/HNUnityFramework/HybridCLR Build Settings")]
        public static void CreateAsset()
        {
            var settings = CreateInstance<HybridCLRBuildSettings>();
            ProjectWindowUtil.CreateAsset(settings, "HybridCLRBuildSettings.asset");
        }
    }
}
