#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Capability.Debug
{
    /// <summary>
    /// 调试模块，将一组相关的日志通道与调试命令组织到一个命名模块中。
    /// </summary>
    /// <remarks>
    /// DebugModule 是模块化调试体系中的组织单元。每个模块拥有一个唯一名称，
    /// 以及它所提供的日志通道和调试命令集合。创建后不可变。
    /// </remarks>
    public class DebugModule
    {
        private readonly List<ILogChannel> _channels;
        private readonly List<IDebugCommand> _commands;

        /// <summary>
        /// 模块名称，如 "Pool", "Network", "Audio" 等。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 模块包含的日志通道集合（只读）。
        /// </summary>
        public IReadOnlyList<ILogChannel> Channels { get; }

        /// <summary>
        /// 模块包含的调试命令集合（只读）。
        /// </summary>
        public IReadOnlyList<IDebugCommand> Commands { get; }

        /// <summary>
        /// 构造一个调试模块实例。
        /// </summary>
        /// <param name="name">模块名称。</param>
        /// <param name="channels">日志通道数组，为 <c>null</c> 时自动转为空集合。</param>
        /// <param name="commands">调试命令数组，为 <c>null</c> 时自动转为空集合。</param>
        public DebugModule(string name, ILogChannel[]? channels, IDebugCommand[]? commands)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            _channels = channels != null ? new List<ILogChannel>(channels) : new List<ILogChannel>();
            _commands = commands != null ? new List<IDebugCommand>(commands) : new List<IDebugCommand>();

            Channels = _channels.AsReadOnly();
            Commands = _commands.AsReadOnly();
        }
    }
}
