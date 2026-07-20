using System;
using System.IO;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建后处理步骤。执行构建完成后的清理和归档操作。
    /// </summary>
    public class PostBuildStep : IBuildStep
    {
        public string StepName => "Post-Build Processing";

        public string Description => "Executes post-build cleanup and archiving.";

        public bool Execute(BuildContext context)
        {
            context.StepLogs.Add("Running post-build processing...");

            // 记录构建信息到日志
            if (context.CustomData.TryGetValue("BuildReport", out var reportObj))
            {
                context.StepLogs.Add("Build report available for manifest generation.");
            }

            // 清理临时文件
            string tempPath = Path.Combine(Application.dataPath, "../Temp/AddressablesBuild");
            if (Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, true);
                    context.StepLogs.Add("Cleaned up temporary build files.");
                }
                catch (Exception ex)
                {
                    context.StepLogs.Add($"Warning: Could not clean temp files: {ex.Message}");
                }
            }

            context.StepLogs.Add("Post-build processing completed.");
            return true;
        }
    }
}
