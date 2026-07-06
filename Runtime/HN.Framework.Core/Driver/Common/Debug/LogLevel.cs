#nullable enable

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 日志等级枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试信息
        /// </summary>
        Debug,
        /// <summary>
        /// 普通信息
        /// </summary>
        Info,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 致命错误
        /// </summary>
        Fatal
    }
}
