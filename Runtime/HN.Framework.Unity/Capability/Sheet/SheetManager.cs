#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Driver.Common.Serialization;
using HN.Framework.Core.Level.Logic.Sheet;

namespace HN.Framework.Unity.Capability.Sheet
{
    /// <summary>
    /// 配置表管理器默认实现。
    /// 内部使用 <see cref="Dictionary{TKey, TValue}"/> 存储表名到表实例的映射。
    /// 支持从二进制数据反序列化经由 <see cref="ISheetRegistrar"/> 注册的表集合。
    /// </summary>
    public class SheetManager : ISheetManager
    {
        private readonly Dictionary<string, object> _tables = new();

        /// <inheritdoc/>
        public IConfigTable<TKey, TRow> GetTable<TKey, TRow>(string tableName)
        {
            if (_tables.TryGetValue(tableName, out var table))
                return (IConfigTable<TKey, TRow>)table;

            throw new KeyNotFoundException($"Table '{tableName}' not registered.");
        }

        /// <inheritdoc/>
        public void RegisterTable<TKey, TRow>(string tableName, IConfigTable<TKey, TRow> table)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException(nameof(tableName));

            _tables.Add(tableName, table);
        }

        /// <inheritdoc/>
        public bool HasTable(string tableName) => _tables.ContainsKey(tableName);

        /// <summary>
        /// 从 MemoryPack 二进制数据反序列化并注册表集合。
        /// </summary>
        /// <typeparam name="TTables">
        /// 需实现 <see cref="ISheetRegistrar"/> 的 Tables 类。
        /// </typeparam>
        /// <param name="data">MemoryPack 序列化的二进制数据。</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> 为 null。</exception>
        public void LoadAllFromBinary<TTables>(byte[] data) where TTables : class, ISheetRegistrar
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var tables = MemoryPackSerializer.Deserialize<TTables>(data);
            tables.RegisterTo(this);
        }
    }
}
