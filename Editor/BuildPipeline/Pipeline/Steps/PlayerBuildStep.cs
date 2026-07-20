using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// Player 构建步骤。调用 Unity BuildPipeline.BuildPlayer。
    /// </summary>
    public class PlayerBuildStep : IBuildStep
    {
        public string StepName => "Player Build";

        public string Description => "Builds the Unity player for the target platform.";

        public bool Execute(BuildContext context)
        {
            context.StepLogs.Add($"Starting player build for {context.BuildTarget}...");

            try
            {
                string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(
                    EditorBuildSettings.scenes);

                if (scenes == null || scenes.Length == 0)
                {
                    Debug.LogError("[PlayerBuildStep] No scenes in Build Settings.");
                    context.StepLogs.Add("No scenes configured in Build Settings.");
                    return false;
                }

                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = context.OutputPath,
                    target = context.BuildTarget,
                    targetGroup = context.BuildTargetGroup,
                    options = BuildOptions.None,
                };

                BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
                bool success = report.summary.result == BuildResult.Succeeded;

                context.StepLogs.Add(
                    $"Player build {(success ? "succeeded" : "failed")}. " +
                    $"Size: {report.summary.totalSize} bytes, " +
                    $"Time: {report.summary.totalTime.TotalSeconds:F1}s");

                if (!success)
                {
                    foreach (var step in report.steps)
                    {
                        if (step.messages != null)
                        {
                            foreach (var msg in step.messages)
                            {
                                if (msg.type == LogType.Error || msg.type == LogType.Exception)
                                {
                                    context.StepLogs.Add($"Build error: {msg.content}");
                                }
                            }
                        }
                    }
                }

                // 存储构建报告到 context
                context.CustomData["BuildReport"] = report;

                return success;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerBuildStep] Failed: {ex.Message}");
                context.StepLogs.Add($"Player build FAILED: {ex.Message}");
                return false;
            }
        }
    }
}
