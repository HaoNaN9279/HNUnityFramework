#nullable enable

using System;
using System.Collections.Generic;

namespace HN.Framework.Core.Driver.Common.Debug
{
    /// <summary>
    /// 调试中枢实现，管理日志通道和调试命令的注册与日志存储。
    /// DebugHub 仅负责存储和事件分发，不直接输出到任何 ILogProvider。
    /// </summary>
    public class DebugHub : IDebugHub
    {
        private const int MaxRecentEntries = 100;

        private readonly Dictionary<string, ILogChannel> _channels;
        private readonly Dictionary<string, IDebugCommand> _commands;
        private readonly List<LogEntry> _recentEntries;

        /// <summary>
        /// 获取已注册的日志通道字典。
        /// </summary>
        public IReadOnlyDictionary<string, ILogChannel> Channels => _channels;

        /// <summary>
        /// 获取已注册的调试命令字典。
        /// </summary>
        public IReadOnlyDictionary<string, IDebugCommand> Commands => _commands;

        /// <summary>
        /// 获取最近的日志条目列表（环形缓冲区，最多 <see cref="MaxRecentEntries"/> 条）。
        /// </summary>
        public IReadOnlyList<LogEntry> RecentEntries => _recentEntries.AsReadOnly();

        /// <summary>
        /// 当新日志条目产生时触发。
        /// </summary>
        public event Action<LogEntry>? OnLog;

        /// <summary>
        /// 构造一个新的调试中枢实例，初始化所有内部存储。
        /// </summary>
        public DebugHub()
        {
            _channels = new Dictionary<string, ILogChannel>();
            _commands = new Dictionary<string, IDebugCommand>();
            _recentEntries = new List<LogEntry>();
        }

        /// <summary>
        /// 注册日志通道。若同名通道已存在，则静默覆盖。
        /// </summary>
        /// <param name="channel">日志通道实例</param>
        public void RegisterChannel(ILogChannel channel)
        {
            _channels[channel.Name] = channel;
        }

        /// <summary>
        /// 注册调试命令。若同名命令已存在，则静默覆盖。
        /// </summary>
        /// <param name="command">调试命令实例</param>
        public void RegisterCommand(IDebugCommand command)
        {
            _commands[command.Name] = command;
        }

        /// <summary>
        /// 输出一条日志。创建 LogEntry 后加入环形缓冲区，并触发 <see cref="OnLog"/> 事件。
        /// DebugHub 不直接输出到 ILogProvider，仅存储和分发。
        /// </summary>
        /// <param name="level">日志等级</param>
        /// <param name="channel">日志通道名称</param>
        /// <param name="message">日志消息</param>
        /// <param name="context">可选的日志上下文</param>
        public void Log(LogLevel level, string channel, string message, object? context = null)
        {
            var entry = new LogEntry(DateTime.UtcNow, channel, level, message, context);
            _recentEntries.Add(entry);

            if (_recentEntries.Count > MaxRecentEntries)
            {
                _recentEntries.RemoveAt(0);
            }

            OnLog?.Invoke(entry);
        }
    }
}
