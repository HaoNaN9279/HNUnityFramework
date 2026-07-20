using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// Addressables 构建步骤。
    /// 调用 Addressables 的 Player Build 管线。
    /// </summary>
    public class AddressablesBuildStep : IBuildStep
    {
        public string StepName => "Addressables Build";

        public string Description => "Builds Addressables content for the target platform.";

        public bool Execute(BuildContext context)
        {
            context.StepLogs.Add("Starting Addressables build...");

            try
            {
                // 通过反射调用 Addressables 构建 API（弱依赖）
                var builderType = Type.GetType(
                    "UnityEditor.AddressableAssets.Build.AddressablesPlayerBuildRunner, Unity.Addressables.Editor",
                    false);

                if (builderType == null)
                {
                    // 旧版 API
                    builderType = Type.GetType(
                        "UnityEditor.AddressableAssets.Build.DataBuilders.AddressablesPlayerBuildRunner, Unity.Addressables.Editor",
                        false);
                }

                if (builderType != null)
                {
                    var buildMethod = builderType.GetMethod("BuildAllPlayers",
                        BindingFlags.Public | BindingFlags.Static);
                    if (buildMethod != null)
                    {
                        buildMethod.Invoke(null, new object[] { context.BuildTarget });
                        context.StepLogs.Add("Addressables build completed.");
                        return true;
                    }
                }

                // Fallback: 调用菜单项
                EditorApplication.ExecuteMenuItem("Assets/Addressables/Player Build/Build Player Content");
                context.StepLogs.Add("Addressables build triggered via menu item.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressablesBuildStep] Failed: {ex.Message}");
                context.StepLogs.Add($"Addressables build FAILED: {ex.Message}");
                return false;
            }
        }
    }
}
