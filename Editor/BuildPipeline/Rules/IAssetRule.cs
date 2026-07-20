using System;
using System.Collections.Generic;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源质检规则接口。由框架定义，供项目端实现自定义规则。
    /// </summary>
    public interface IAssetRule
    {
        /// <summary>
        /// 规则名称。
        /// </summary>
        string RuleName { get; }

        /// <summary>
        /// 规则描述。
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 目标 AssetImporter 类型。
        /// 返回 null 表示适用于所有导入器类型。
        /// </summary>
        Type TargetImporterType { get; }

        /// <summary>
        /// 对指定资源执行校验。
        /// </summary>
        /// <param name="assetPath">项目内资源路径（如 "Assets/Textures/foo.png"）</param>
        /// <returns>校验结果集合</returns>
        IReadOnlyList<RuleResult> Validate(string assetPath);
    }
}
