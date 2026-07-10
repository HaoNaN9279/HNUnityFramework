#nullable enable

using System.Collections.Generic;

namespace HN.Framework.Core.Capability.Localization
{
    /// <summary>
    /// 单个语言区域的本地化字符串表，通过键查找对应的本地化文本。
    /// </summary>
    public class StringTable
    {
        private readonly Dictionary<string, string> _entries;

        /// <summary>
        /// 初始化一个空的 <see cref="StringTable"/> 实例。
        /// </summary>
        public StringTable()
        {
            _entries = new Dictionary<string, string>();
        }

        /// <summary>
        /// 使用指定的键值对集合初始化 <see cref="StringTable"/> 实例。
        /// </summary>
        /// <param name="entries">本地化键到文本的映射字典。</param>
        public StringTable(Dictionary<string, string> entries)
        {
            _entries = new Dictionary<string, string>(entries);
        }

        /// <summary>
        /// 获取当前表中的条目数量。
        /// </summary>
        public int Count => _entries.Count;

        /// <summary>
        /// 根据键获取本地化字符串。如果键不存在，返回键本身以优雅降级。
        /// </summary>
        /// <param name="key">本地化键。</param>
        /// <returns>对应的本地化文本，或键本身（降级）。</returns>
        public string GetString(string key)
        {
            return _entries.TryGetValue(key, out var value) ? value : key;
        }

        /// <summary>
        /// 尝试根据键获取本地化字符串。
        /// </summary>
        /// <param name="key">本地化键。</param>
        /// <param name="value">输出参数，获取到的本地化文本。</param>
        /// <returns>如果键存在则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool TryGetString(string key, out string value)
        {
            return _entries.TryGetValue(key, out value!);
        }

        /// <summary>
        /// 添加或更新一个本地化键值对。
        /// </summary>
        /// <param name="key">本地化键。</param>
        /// <param name="value">本地化文本。</param>
        public void AddEntry(string key, string value)
        {
            _entries[key] = value;
        }

        /// <summary>
        /// 检查指定的本地化键是否存在。
        /// </summary>
        /// <param name="key">本地化键。</param>
        /// <returns>如果键存在则返回 <c>true</c>。</returns>
        public bool ContainsKey(string key)
        {
            return _entries.ContainsKey(key);
        }
    }
}
