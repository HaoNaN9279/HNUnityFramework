namespace HN.Framework.Editor.BuildPipeline
{
    /// <summary>
    /// 资源质检规则的严重度等级。
    /// </summary>
    public enum RuleSeverity
    {
        /// <summary>
        /// 信息，仅提示作用。
        /// </summary>
        Info = 0,

        /// <summary>
        /// 警告，建议修复但不强制。
        /// </summary>
        Warning = 1,

        /// <summary>
        /// 错误，必须修复。
        /// </summary>
        Error = 2,

        /// <summary>
        /// 致命，构建流程强制中止。
        /// </summary>
        Fatal = 3,
    }
}
