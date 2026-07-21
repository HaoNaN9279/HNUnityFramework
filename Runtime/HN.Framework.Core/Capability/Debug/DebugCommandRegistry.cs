#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common.Debug;

namespace HN.Framework.Core.Capability.Debug
{
    /// <summary>
    /// 调试命令注册表，提供对命令字典的查询能力。
    /// </summary>
    /// <remarks>
    /// DebugCommandRegistry 是一个纯粹的查询层，封装 <see cref="IReadOnlyDictionary{TKey, TValue}"/>，
    /// 不维护独立的命令存储。它提供精确查找和前缀搜索两种查询方式。
    /// <para>
    /// 命令存储本身由外部管理（如 <see cref="DebugHub"/>），本类的职责仅限于查询。
    /// </para>
    /// </remarks>
    public class DebugCommandRegistry
    {
        private readonly IReadOnlyDictionary<string, IDebugCommand> _commands;

        /// <summary>
        /// 获取封装的基础命令字典（只读）。
        /// </summary>
        public IReadOnlyDictionary<string, IDebugCommand> Commands => _commands;

        /// <summary>
        /// 获取当前字典中的命令总数。
        /// </summary>
        public int Count => _commands.Count;

        /// <summary>
        /// 构造一个命令注册表，封装给定的命令字典。
        /// </summary>
        /// <param name="commands">命令字典引用，不会创建副本。</param>
        public DebugCommandRegistry(IReadOnlyDictionary<string, IDebugCommand> commands)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }

        /// <summary>
        /// 按名称精确查找命令。
        /// </summary>
        /// <param name="name">命令名称，如 "pool.show"。</param>
        /// <returns>找到的命令；若不存在则返回 <c>null</c>。</returns>
        public IDebugCommand? Find(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            _commands.TryGetValue(name, out var command);
            return command;
        }

        /// <summary>
        /// 按名称前缀搜索所有匹配的命令（忽略大小写）。
        /// </summary>
        /// <param name="prefix">搜索前缀。</param>
        /// <returns>所有名称以前缀开头的命令列表，无匹配时返回空列表。</returns>
        public IReadOnlyList<IDebugCommand> Search(string prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            var results = new List<IDebugCommand>();
            foreach (var kvp in _commands)
            {
                if (kvp.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(kvp.Value);
                }
            }
            return results.AsReadOnly();
        }
    }
}
