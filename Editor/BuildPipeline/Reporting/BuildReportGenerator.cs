using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建报告生成器。将构建清单和校验结果转换为 JSON 或 Markdown 格式。
    /// </summary>
    public static class BuildReportGenerator
    {
        /// <summary>生成 JSON 格式的构建报告</summary>
        /// <param name="manifest">构建清单</param>
        /// <param name="validationResults">校验结果列表</param>
        /// <returns>JSON 字符串</returns>
        public static string GenerateJson(BuildManifest manifest, List<RuleResult> validationResults)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"buildTime\": \"{EscapeJson(manifest.BuildTime)}\",");
            sb.AppendLine($"  \"version\": \"{EscapeJson(manifest.Version)}\",");
            sb.AppendLine($"  \"branch\": \"{EscapeJson(manifest.Branch)}\",");
            sb.AppendLine($"  \"commitHash\": \"{EscapeJson(manifest.CommitHash)}\",");
            sb.AppendLine($"  \"platform\": \"{EscapeJson(manifest.Platform)}\",");
            sb.AppendLine($"  \"buildSize\": {manifest.BuildSize},");
            sb.AppendLine($"  \"outputPath\": \"{EscapeJson(manifest.OutputPath)}\",");
            sb.AppendLine($"  \"buildNumber\": {manifest.BuildNumber},");

            // 校验结果统计
            int errorCount = 0;
            int warningCount = 0;
            if (validationResults != null)
            {
                foreach (var result in validationResults)
                {
                    if (result.Severity >= RuleSeverity.Error)
                    {
                        errorCount++;
                    }
                    else if (result.Severity == RuleSeverity.Warning)
                    {
                        warningCount++;
                    }
                }
            }

            sb.AppendLine($"  \"validationErrorCount\": {errorCount},");
            sb.AppendLine($"  \"validationWarningCount\": {warningCount},");

            // 校验结果数组
            sb.AppendLine("  \"validationResults\": [");
            if (validationResults != null && validationResults.Count > 0)
            {
                for (int i = 0; i < validationResults.Count; i++)
                {
                    var result = validationResults[i];
                    string comma = (i < validationResults.Count - 1) ? "," : "";
                    sb.AppendLine("    {");
                    sb.AppendLine($"      \"assetPath\": \"{EscapeJson(result.AssetPath)}\",");
                    sb.AppendLine($"      \"message\": \"{EscapeJson(result.Message)}\",");
                    sb.AppendLine($"      \"severity\": \"{result.Severity}\",");
                    sb.AppendLine($"      \"ruleId\": \"{EscapeJson(result.RuleName)}\"");
                    sb.AppendLine($"    }}{comma}");
                }
            }
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        /// <summary>生成 Markdown 格式的构建报告</summary>
        /// <param name="manifest">构建清单</param>
        /// <param name="validationResults">校验结果列表</param>
        /// <returns>Markdown 字符串</returns>
        public static string GenerateMarkdown(BuildManifest manifest, List<RuleResult> validationResults)
        {
            if (manifest == null)
            {
                throw new ArgumentNullException(nameof(manifest));
            }

            var sb = new StringBuilder();

            sb.AppendLine("# Build Report");
            sb.AppendLine();
            sb.AppendLine($"**Build Time:** {manifest.BuildTime}  ");
            sb.AppendLine($"**Version:** {manifest.Version}  ");
            sb.AppendLine($"**Build Number:** {manifest.BuildNumber}  ");
            sb.AppendLine($"**Platform:** {manifest.Platform}  ");

            if (!string.IsNullOrEmpty(manifest.Branch))
            {
                sb.AppendLine($"**Branch:** {manifest.Branch}  ");
            }

            if (!string.IsNullOrEmpty(manifest.CommitHash))
            {
                sb.AppendLine($"**Commit:** {manifest.CommitHash}  ");
            }

            sb.AppendLine($"**Output Path:** `{manifest.OutputPath}`  ");
            sb.AppendLine($"**Build Size:** {FormatFileSize(manifest.BuildSize)}  ");
            sb.AppendLine();

            // 校验结果统计
            if (validationResults != null && validationResults.Count > 0)
            {
                int errorCount = 0;
                int warningCount = 0;
                foreach (var result in validationResults)
                {
                    if (result.Severity >= RuleSeverity.Error)
                    {
                        errorCount++;
                    }
                    else if (result.Severity == RuleSeverity.Warning)
                    {
                        warningCount++;
                    }
                }

                sb.AppendLine("## Validation Results");
                sb.AppendLine();
                sb.AppendLine($"- **Errors:** {errorCount}");
                sb.AppendLine($"- **Warnings:** {warningCount}");
                sb.AppendLine();
                sb.AppendLine("| Severity | Asset | Message |");
                sb.AppendLine("|----------|-------|---------|");

                foreach (var result in validationResults)
                {
                    string severityIcon = result.Severity >= RuleSeverity.Error ? "❌ Error" : "⚠️ Warning";
                    sb.AppendLine($"| {severityIcon} | `{result.AssetPath}` | {result.Message} |");
                }

                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine($"*Report generated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");

            return sb.ToString();
        }

        /// <summary>转义 JSON 字符串中的特殊字符</summary>
        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        /// <summary>格式化文件大小为人类可读的字符串</summary>
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }
    }
}
