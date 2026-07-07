using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace HN.Framework.Editor.Scripting
{
    /// <summary>
    /// HybridCLR 构建管线处理器，在 Unity 构建过程中自动处理 AOT 元数据生成和原生库拷贝。
    /// </summary>
    public class HybridCLRBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>
        /// 回调执行顺序。数值越小越先执行。
        /// </summary>
        public int callbackOrder => 0;

        /// <summary>
        /// 构建前回调：自动生成 AOT 元数据，验证 HybridCLR 安装状态。
        /// </summary>
        /// <param name="report">Unity 构建报告。</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = HybridCLRMetadataGenerator.LoadSettings();
            if (settings == null)
            {
                return;
            }

            if (settings.autoGenerateMetadata)
            {
                UnityEngine.Debug.Log("[HybridCLRBuildProcessor] Auto-generating AOT metadata...");
                HybridCLRMetadataGenerator.GenerateAOTMetadata(settings);
            }

            HybridCLRNativeLibManager.ValidateHybridCLRInstallation();
        }

        /// <summary>
        /// 构建后回调：自动拷贝 HybridCLR 原生库到构建输出目录。
        /// </summary>
        /// <param name="report">Unity 构建报告。</param>
        public void OnPostprocessBuild(BuildReport report)
        {
            var settings = HybridCLRMetadataGenerator.LoadSettings();
            if (settings == null)
            {
                return;
            }

            if (settings.autoCopyNativeLibs)
            {
                var outputDir = Path.GetDirectoryName(report.summary.outputPath);
                HybridCLRNativeLibManager.CopyNativeLibsToBuild(outputDir);
            }
        }
    }
}
