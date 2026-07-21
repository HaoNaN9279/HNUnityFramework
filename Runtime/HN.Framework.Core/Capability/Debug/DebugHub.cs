#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Capability.Debug
{
    /// <summary>增强版调试中枢，继承 Driver DebugHub，支持模块注册、命令执行和 ILogProvider 桥接。</summary>
    public class DebugHub : Driver.Common.Debug.DebugHub
    {
        private readonly List<DebugModule> _modules;
        private readonly DebugCommandRegistry _commandRegistry;
        private Action<LogEntry>? _logForwarder;

        public DebugHub() : base()
        {
            _modules = new List<DebugModule>();
            // CRITICAL: Pass base class's public Commands dictionary (IReadOnlyDictionary) to the query layer
            // This is the Momus F2 fix - DebugCommandRegistry is a QUERY LAYER, not an independent store
            _commandRegistry = new DebugCommandRegistry(this.Commands);
        }

        /// <summary>
        /// 已注册的调试模块列表（只读）。
        /// </summary>
        public IReadOnlyList<DebugModule> Modules => _modules.AsReadOnly();

        /// <summary>
        /// 注册一个调试模块，将其所有通道和命令注册到中枢。
        /// </summary>
        /// <param name="module">调试模块实例。</param>
        public void RegisterModule(DebugModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));
            _modules.Add(module);

            // Use base class's PUBLIC RegisterChannel/RegisterCommand methods
            // Base class fields are private, subclasses must use public API
            foreach (var channel in module.Channels)
                RegisterChannel(channel);
            foreach (var command in module.Commands)
                RegisterCommand(command);
        }

        /// <summary>
        /// 按名称查找并执行调试命令。
        /// </summary>
        /// <param name="name">命令名称。</param>
        /// <param name="args">命令参数。</param>
        /// <returns>找到并执行返回 true；未找到或执行异常返回 false。</returns>
        public bool ExecuteCommand(string name, string[] args)
        {
            var command = _commandRegistry.Find(name);
            if (command == null) return false;
            try { command.Execute(args); return true; }
            catch { return false; }
        }

        /// <summary>
        /// 按前缀搜索调试命令（用于自动补全）。
        /// </summary>
        /// <param name="prefix">搜索前缀。</param>
        /// <returns>匹配的命令列表。</returns>
        public IReadOnlyList<IDebugCommand> SearchCommands(string prefix)
        {
            return _commandRegistry.Search(prefix);
        }

        /// <summary>
        /// 设置日志提供者。当设置后，每次 Log() 产生的日志条目自动转发到提供者。
        /// 支持多次调用且不泄漏订阅（自动取消旧订阅）。
        /// 传入 null 可取消转发。
        /// </summary>
        /// <param name="provider">日志提供者实例，或 null 取消转发。</param>
        public void SetLogProvider(ILogProvider? provider)
        {
            // Unsubscribe old forwarder first to prevent subscription leak (Momus F5 fix)
            if (_logForwarder != null)
            {
                OnLog -= _logForwarder;
                _logForwarder = null;
            }

            if (provider != null)
            {
                // NOTE: ILogProvider does NOT have Log(LogEntry) overload
                // Must destructure LogEntry into individual parameters (Momus F6 fix)
                _logForwarder = entry => provider.Log(entry.Level, entry.Channel, entry.Message, entry.Context);
                OnLog += _logForwarder;
            }
        }
    }
}
