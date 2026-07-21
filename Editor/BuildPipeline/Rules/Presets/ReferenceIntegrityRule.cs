using System;
using System.Collections.Generic;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 引用完整性检查规则。
    /// 检测资源引用链中目标是否存在。
    /// </summary>
    public class ReferenceIntegrityRule : IAssetRule
    {
        /// <inheritdoc />
        public string RuleName => "Reference Integrity Check";

        /// <inheritdoc />
        public string Description => "Checks that all asset references in the dependency chain point to existing assets.";

        /// <inheritdoc />
        public Type TargetImporterType => null;

        /// <inheritdoc />
        public IReadOnlyList<RuleResult> Validate(string assetPath)
        {
            var results = new List<RuleResult>();

            // 跳过文件夹
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            // 检查资源自身是否存在
            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath)))
            {
                results.Add(RuleResult.Fail(
                    RuleName,
                    assetPath,
                    RuleSeverity.Error,
                    $"Asset path does not exist in the database: {assetPath}"));
                return results;
            }

            // 获取依赖列表并检查每个依赖是否存在
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);

            foreach (string depPath in dependencies)
            {
                if (string.IsNullOrEmpty(depPath))
                {
                    continue;
                }

                // 跳过自身
                if (string.Equals(depPath, assetPath, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string guid = AssetDatabase.AssetPathToGUID(depPath);
                if (string.IsNullOrEmpty(guid))
                {
                    results.Add(RuleResult.Fail(
                        RuleName,
                        assetPath,
                        RuleSeverity.Error,
                        $"Missing dependency: {depPath} (referenced by {assetPath})"));
                }
            }

            if (results.Count == 0)
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
            }

            return results;
        }
    }
}
