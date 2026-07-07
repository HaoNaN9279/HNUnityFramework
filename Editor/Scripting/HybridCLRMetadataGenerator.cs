using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR AOT 元数据生成器，封装 HybridCLR 的标准生成流程。
    /// </summary>
    public static class HybridCLRMetadataGenerator
    {
        /// <summary>
        /// 菜单项：一键生成 AOT 元数据。
        /// </summary>
        [MenuItem("Tools/HNUnityFramework/HybridCLR/Generate AOT Metadata")]
        public static void GenerateAOTMetadata()
        {
            var settings = LoadSettings();
            if (settings == null)
            {
                EditorUtility.DisplayDialog(
                    "HybridCLR",
                    "Please create HybridCLRBuildSettings first (Assets/Create/HNUnityFramework/HybridCLR Build Settings)",
                    "OK");
                return;
            }

            GenerateAOTMetadata(settings);
        }

        /// <summary>
        /// 使用 HybridCLR 标准 API 生成 AOT 元数据 (PrebuildCommand.GenerateAll)。
        /// </summary>
        /// <param name="settings">HybridCLR 构建设置资产。</param>
        public static void GenerateAOTMetadata(HybridCLRBuildSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            try
            {
                // HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll()
                // 使用反射调用以确保兼容不同版本的 HybridCLR 包
                var prebuildType = Type.GetType(
                    "HybridCLR.Editor.Commands.PrebuildCommand, HybridCLR.Editor");
                if (prebuildType == null)
                {
                    UnityEngine.Debug.LogError(
                        "[HybridCLR] PrebuildCommand type not found. Is the HybridCLR package installed?");
                    EditorUtility.DisplayDialog(
                        "HybridCLR Error",
                        "PrebuildCommand type not found. Is the HybridCLR package installed?",
                        "OK");
                    return;
                }

                var method = prebuildType.GetMethod(
                    "GenerateAll",
                    BindingFlags.Static | BindingFlags.Public);
                if (method == null)
                {
                    UnityEngine.Debug.LogError(
                        "[HybridCLR] PrebuildCommand.GenerateAll method not found.");
                    EditorUtility.DisplayDialog(
                        "HybridCLR Error",
                        "PrebuildCommand.GenerateAll method not found. Check HybridCLR version.",
                        "OK");
                    return;
                }

                method.Invoke(null, null);
                UnityEngine.Debug.Log(
                    "[HybridCLR] AOT metadata generation completed via PrebuildCommand.GenerateAll().");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(
                    $"[HybridCLR] AOT metadata generation failed: {e.Message}");
                EditorUtility.DisplayDialog(
                    "HybridCLR Error",
                    $"AOT metadata generation failed:\n{e.Message}",
                    "OK");
            }
        }

        /// <summary>
        /// 从项目中加载 HybridCLRBuildSettings 资产。
        /// </summary>
        /// <returns>第一个找到的设置资产；未找到时返回 null。</returns>
        public static HybridCLRBuildSettings LoadSettings()
        {
            var guids = AssetDatabase.FindAssets("t:HybridCLRBuildSettings");
            if (guids.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<HybridCLRBuildSettings>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
            }

            return null;
        }
    }
}
