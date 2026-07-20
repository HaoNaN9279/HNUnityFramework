using System;

namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源质检规则的检查结果。
    /// </summary>
    public struct RuleResult : IEquatable<RuleResult>
    {
        /// <summary>
        /// 规则名称。
        /// </summary>
        public string RuleName { get; set; }

        /// <summary>
        /// 严重度。
        /// </summary>
        public RuleSeverity Severity { get; set; }

        /// <summary>
        /// 被检查的资源路径。
        /// </summary>
        public string AssetPath { get; set; }

        /// <summary>
        /// 检查消息描述。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否通过检查（Severity 低于 Error 视为通过）。
        /// </summary>
        public bool Passed => Severity < RuleSeverity.Error;

        /// <summary>
        /// 创建一条通过的检查结果。
        /// </summary>
        /// <param name="ruleName">规则名称</param>
        /// <param name="assetPath">资源路径</param>
        /// <returns>检查结果</returns>
        public static RuleResult Pass(string ruleName, string assetPath)
        {
            return new RuleResult
            {
                RuleName = ruleName,
                Severity = RuleSeverity.Info,
                AssetPath = assetPath,
                Message = "Passed.",
            };
        }

        /// <summary>
        /// 创建一条失败的检查结果。
        /// </summary>
        /// <param name="ruleName">规则名称</param>
        /// <param name="assetPath">资源路径</param>
        /// <param name="severity">严重度</param>
        /// <param name="message">失败描述</param>
        /// <returns>检查结果</returns>
        public static RuleResult Fail(
            string ruleName,
            string assetPath,
            RuleSeverity severity,
            string message)
        {
            return new RuleResult
            {
                RuleName = ruleName,
                Severity = severity,
                AssetPath = assetPath,
                Message = message,
            };
        }

        /// <inheritdoc />
        public bool Equals(RuleResult other)
        {
            return string.Equals(RuleName, other.RuleName, StringComparison.Ordinal)
                && string.Equals(AssetPath, other.AssetPath, StringComparison.Ordinal)
                && Severity == other.Severity
                && string.Equals(Message, other.Message, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is RuleResult other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = RuleName != null ? StringComparer.Ordinal.GetHashCode(RuleName) : 0;
                hashCode = (hashCode * 397)
                    ^ (AssetPath != null ? StringComparer.Ordinal.GetHashCode(AssetPath) : 0);
                hashCode = (hashCode * 397) ^ (int)Severity;
                hashCode = (hashCode * 397)
                    ^ (Message != null ? StringComparer.Ordinal.GetHashCode(Message) : 0);
                return hashCode;
            }
        }
    }
}
