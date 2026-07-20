using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 命名规范检查规则。
    /// 基于可配置的正则表达式校验文件名是否符合命名规范。
    /// </summary>
    public class NamingConventionRule : IAssetRule
    {
        private readonly string m_Pattern;
        private readonly string m_Description;
        private readonly Regex m_Regex;

        /// <summary>
        /// 使用指定的正则模式创建命名规范规则。
        /// </summary>
        /// <param name="pattern">正则表达式模式</param>
        /// <param name="description">规则描述</param>
        public NamingConventionRule(string pattern, string description)
        {
            m_Pattern = pattern;
            m_Description = description;
            m_Regex = new Regex(pattern, RegexOptions.Compiled);
        }

        /// <summary>
        /// 创建默认命名规范规则（允许字母数字下划线，以字母开头）。
        /// </summary>
        public static NamingConventionRule CreateDefault()
        {
            return new NamingConventionRule(
                @"^[A-Za-z][A-Za-z0-9_]*$",
                "File names must start with a letter and contain only letters, digits, and underscores.");
        }

        /// <inheritdoc />
        public string RuleName => "Naming Convention Check";

        /// <inheritdoc />
        public string Description => m_Description;

        /// <summary>
        /// 当前正则模式。
        /// </summary>
        public string Pattern => m_Pattern;

        /// <inheritdoc />
        public Type TargetImporterType => null;

        /// <inheritdoc />
        public IReadOnlyList<RuleResult> Validate(string assetPath)
        {
            var results = new List<RuleResult>();

            // 跳过目录
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (string.IsNullOrEmpty(fileName))
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
                return results;
            }

            if (!m_Regex.IsMatch(fileName))
            {
                results.Add(RuleResult.Fail(
                    RuleName,
                    assetPath,
                    RuleSeverity.Warning,
                    $"File name \"{fileName}\" does not match naming pattern: {m_Pattern}"));
            }
            else
            {
                results.Add(RuleResult.Pass(RuleName, assetPath));
            }

            return results;
        }
    }
}
