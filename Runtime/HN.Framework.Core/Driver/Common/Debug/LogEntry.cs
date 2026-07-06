#nullable enable

using System;

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 结构化日志条目，包含日志的完整元数据
    /// </summary>
    public readonly struct LogEntry
    {
        /// <summary>
        /// 日志时间戳
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// 日志通道名称
        /// </summary>
        public string Channel { get; }

        /// <summary>
        /// 日志等级
        /// </summary>
        public LogLevel Level { get; }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 可选的日志上下文
        /// </summary>
        public object? Context { get; }

        /// <summary>
        /// 构造一个日志条目
        /// </summary>
        /// <param name="timestamp">日志时间戳</param>
        /// <param name="channel">日志通道名称</param>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志消息</param>
        /// <param name="context">可选的日志上下文</param>
        public LogEntry(DateTime timestamp, string channel, LogLevel level, string message, object? context = null)
        {
            Timestamp = timestamp;
            Channel = channel;
            Level = level;
            Message = message;
            Context = context;
        }
    }
}
