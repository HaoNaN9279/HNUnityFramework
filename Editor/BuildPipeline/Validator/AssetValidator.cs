using System;
using System.Collections.Generic;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源质检调度器。
    /// 遍历指定目录 → 识别资源 Importer 类型 → 匹配规则 → 执行校验 → 收集结果。
    /// </summary>
    public class AssetValidator
    {
        private readonly List<IAssetRule> m_AdditionalRules;

        /// <summary>
        /// 创建校验器实例。
        /// </summary>
        public AssetValidator()
        {
            m_AdditionalRules = new List<IAssetRule>();
        }

        /// <summary>
        /// 添加额外规则（临时注册，不影响全局注册中心）。
        /// </summary>
        /// <param name="rule">规则实例</param>
        public void AddRule(IAssetRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            m_AdditionalRules.Add(rule);
        }

        /// <summary>
        /// 校验指定路径下的所有资源。
        /// </summary>
        /// <param name="targetPath">目标路径（如 "Assets/Textures"）</param>
        /// <param name="recursive">是否递归子目录</param>
        /// <param name="onProgress">进度回调（当前索引，总数）</param>
        /// <returns>校验结果集合</returns>
        public IReadOnlyList<RuleResult> Validate(
            string targetPath,
            bool recursive = true,
            Action<int, int> onProgress = null)
        {
            var allResults = new List<RuleResult>();

            // 收集所有需要检查的资源
            string[] assetGuids = AssetDatabase.FindAssets("t:Object", new[] { targetPath });
            var assetPaths = new List<string>();

            foreach (string guid in assetGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                // 跳过文件夹
                if (!AssetDatabase.IsValidFolder(path))
                {
                    assetPaths.Add(path);
                }
            }

            int total = assetPaths.Count;
            for (int i = 0; i < total; i++)
            {
                string assetPath = assetPaths[i];
                onProgress?.Invoke(i + 1, total);

                // 获取资源的 Importer 类型
                Type importerType = GetImporterType(assetPath);
                if (importerType == null)
                {
                    continue;
                }

                // 获取匹配的规则：全局规则 + 注册中心规则 + 额外规则
                var rules = GetRulesForAsset(importerType);

                foreach (var rule in rules)
                {
                    IReadOnlyList<RuleResult> ruleResults = rule.Validate(assetPath);
                    allResults.AddRange(ruleResults);
                }
            }

            return allResults;
        }

        /// <summary>
        /// 校验单个资源。
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns>校验结果集合</returns>
        public IReadOnlyList<RuleResult> ValidateSingle(string assetPath)
        {
            var results = new List<RuleResult>();

            Type importerType = GetImporterType(assetPath);
            if (importerType == null)
            {
                return results;
            }

            var rules = GetRulesForAsset(importerType);
            foreach (var rule in rules)
            {
                IReadOnlyList<RuleResult> ruleResults = rule.Validate(assetPath);
                results.AddRange(ruleResults);
            }

            return results;
        }

        private Type GetImporterType(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return null;
            }

            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            return importer?.GetType();
        }

        private IReadOnlyList<IAssetRule> GetRulesForAsset(Type importerType)
        {
            var combined = new List<IAssetRule>();

            // 全局规则
            combined.AddRange(AssetRuleRegistry.GetRulesForImporter(importerType));

            // 额外规则
            combined.AddRange(m_AdditionalRules);

            return combined;
        }
    }
}
