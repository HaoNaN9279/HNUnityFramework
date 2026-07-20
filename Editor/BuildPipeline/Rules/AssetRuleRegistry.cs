using System;
using System.Collections.Generic;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 按 AssetImporter 类型分组的规则注册中心。
    /// 框架预设规则和项目自定义规则统一注册到此中心。
    /// </summary>
    [InitializeOnLoad]
    public static class AssetRuleRegistry
    {
        private static readonly Dictionary<Type, List<IAssetRule>> s_RulesByImporterType;
        private static readonly List<IAssetRule> s_GlobalRules;

        /// <summary>
        /// 静态构造函数：初始化规则容器。
        /// </summary>
        static AssetRuleRegistry()
        {
            s_RulesByImporterType = new Dictionary<Type, List<IAssetRule>>();
            s_GlobalRules = new List<IAssetRule>();
        }

        /// <summary>
        /// 注册一条规则到指定的 AssetImporter 类型。
        /// </summary>
        /// <typeparam name="TImporter">AssetImporter 子类类型</typeparam>
        /// <param name="rule">规则实例</param>
        public static void RegisterRule<TImporter>(IAssetRule rule)
            where TImporter : AssetImporter
        {
            RegisterRule(typeof(TImporter), rule);
        }

        /// <summary>
        /// 注册一条规则到指定的 AssetImporter 类型。
        /// </summary>
        /// <param name="importerType">AssetImporter 子类类型</param>
        /// <param name="rule">规则实例</param>
        public static void RegisterRule(Type importerType, IAssetRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            if (!typeof(AssetImporter).IsAssignableFrom(importerType))
            {
                throw new ArgumentException(
                    $"Type must be a subclass of AssetImporter: {importerType.FullName}",
                    nameof(importerType));
            }

            if (!s_RulesByImporterType.TryGetValue(importerType, out var rules))
            {
                rules = new List<IAssetRule>();
                s_RulesByImporterType[importerType] = rules;
            }

            rules.Add(rule);
        }

        /// <summary>
        /// 注册一条全局规则（适用于所有导入器类型）。
        /// </summary>
        /// <param name="rule">规则实例</param>
        public static void RegisterGlobalRule(IAssetRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            s_GlobalRules.Add(rule);
        }

        /// <summary>
        /// 获取指定 AssetImporter 类型的所有相关规则。
        /// 包含：全局规则 + 指定类型的注册规则。
        /// </summary>
        /// <param name="importerType">AssetImporter 子类类型</param>
        /// <returns>规则列表（只读）</returns>
        public static IReadOnlyList<IAssetRule> GetRulesForImporter(Type importerType)
        {
            var result = new List<IAssetRule>(s_GlobalRules);

            if (s_RulesByImporterType.TryGetValue(importerType, out var rules))
            {
                result.AddRange(rules);
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// 获取所有已注册的规则。
        /// </summary>
        /// <returns>规则列表（只读）</returns>
        public static IReadOnlyList<IAssetRule> GetAllRules()
        {
            var result = new List<IAssetRule>(s_GlobalRules);

            foreach (var kvp in s_RulesByImporterType)
            {
                result.AddRange(kvp.Value);
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// 获取已注册的导入器类型列表。
        /// </summary>
        /// <returns>导入器类型列表</returns>
        public static IReadOnlyList<Type> GetRegisteredImporterTypes()
        {
            return new List<Type>(s_RulesByImporterType.Keys).AsReadOnly();
        }

        /// <summary>
        /// 清空所有已注册的规则。
        /// </summary>
        public static void Clear()
        {
            s_RulesByImporterType.Clear();
            s_GlobalRules.Clear();
        }

        /// <summary>
        /// 获取指定规则类型的已注册规则数量。
        /// </summary>
        /// <param name="importerType">导入器类型</param>
        /// <returns>规则数量</returns>
        public static int GetRuleCount(Type importerType)
        {
            if (s_RulesByImporterType.TryGetValue(importerType, out var rules))
            {
                return rules.Count;
            }

            return 0;
        }

        /// <summary>
        /// 获取全局规则数量。
        /// </summary>
        /// <returns>全局规则数量</returns>
        public static int GetGlobalRuleCount()
        {
            return s_GlobalRules.Count;
        }
    }
}
