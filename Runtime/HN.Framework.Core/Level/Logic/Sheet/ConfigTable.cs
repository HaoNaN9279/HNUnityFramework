#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// 配置表通用实现，使用 <see cref="Dictionary{TKey, TRow}"/> 实现 O(1) 主键查询，
    /// 同时维护行数组实现零分配遍历。
    /// </summary>
    /// <typeparam name="TKey">主键类型。</typeparam>
    /// <typeparam name="TRow">行数据类型。</typeparam>
    public class ConfigTable<TKey, TRow> : IConfigTable<TKey, TRow>
    {
        private readonly Dictionary<TKey, TRow> _dict;
        private readonly TRow[] _rows;

        /// <summary>
        /// 根据行数据枚举和主键选择器构造配置表。
        /// </summary>
        /// <param name="rows">行数据枚举。</param>
        /// <param name="keySelector">从行数据提取主键的委托。</param>
        /// <exception cref="ArgumentNullException"><paramref name="rows"/> 或 <paramref name="keySelector"/> 为 null。</exception>
        /// <exception cref="ArgumentException">行数据中存在重复主键。</exception>
        public ConfigTable(IEnumerable<TRow> rows, Func<TRow, TKey> keySelector)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var list = rows.ToList();
            _rows = list.ToArray();
            _dict = new Dictionary<TKey, TRow>(_rows.Length);

            foreach (var row in _rows)
            {
                var key = keySelector(row);
                _dict.Add(key, row);
            }
        }

        /// <inheritdoc/>
        public TRow Get(TKey key) => _dict[key];

        /// <inheritdoc/>
        public bool TryGet(TKey key, out TRow row) => _dict.TryGetValue(key, out row);

        /// <inheritdoc/>
        public TRow[] GetAll() => _rows;

        /// <inheritdoc/>
        public int Count => _rows.Length;

        /// <inheritdoc/>
        public bool ContainsKey(TKey key) => _dict.ContainsKey(key);
    }
}
