using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建前质检门禁步骤。调用 AssetValidator 进行资源检查，
    /// 根据严重度决定是否阻止构建。
    /// </summary>
    public class PreBuildValidationStep : IBuildStep
    {
        public string StepName => "Pre-Build Validation";

        public string Description => "Validates all assets before build.";

        public bool Execute(BuildContext context)
        {
            if (!context.EnableValidation)
            {
                context.StepLogs.Add("Validation disabled, skipping.");
                return true;
            }

            var validator = new AssetValidator();
            var results = validator.Validate("Assets", true);

            bool hasErrors = false;
            foreach (var result in results)
            {
                if (result.Severity >= RuleSeverity.Error)
                {
                    hasErrors = true;
                    Debug.LogError($"[PreBuildValidation] {result.AssetPath}: {result.Message}");
                    context.StepLogs.Add($"ERROR: {result.AssetPath} - {result.Message}");
                }
                else if (result.Severity == RuleSeverity.Warning)
                {
                    Debug.LogWarning($"[PreBuildValidation] {result.AssetPath}: {result.Message}");
                    context.StepLogs.Add($"WARN: {result.AssetPath} - {result.Message}");
                }
            }

            if (hasErrors)
            {
                var settings = BuildPipelineSettings.GetOrCreateSettings();
                if (settings.BlockBuildOnValidationFailure)
                {
                    Debug.LogError("[PreBuildValidation] Build blocked due to validation errors.");
                    context.StepLogs.Add("Build BLOCKED due to validation errors.");
                    return false;
                }
            }

            context.StepLogs.Add("Pre-build validation completed.");
            return true;
        }
    }
}
