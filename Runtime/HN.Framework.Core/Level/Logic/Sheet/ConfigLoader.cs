#nullable enable

using System;
using HN.Framework.Core.Driver.Common.Serialization;

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// 配置表加载器，提供从二进制数据流反序列化配置表的能力。
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// 从字节数组反序列化指定类型的对象。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="data">序列化的二进制数据。</param>
        /// <returns>反序列化后的对象。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> 为 null。</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> 为空数组。</exception>
        public static T LoadFromBytes<T>(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0)
                throw new ArgumentException("Data cannot be empty.", nameof(data));

            return MemoryPackSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// 从字节数组反序列化行数据并构建配置表。
        /// </summary>
        /// <typeparam name="TKey">主键类型。</typeparam>
        /// <typeparam name="TRow">行数据类型。</typeparam>
        /// <param name="data">序列化的行数组二进制数据。</param>
        /// <param name="keySelector">从行数据提取主键的委托。</param>
        /// <returns>配置表实例。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> 或 <paramref name="keySelector"/> 为 null。</exception>
        /// <exception cref="ArgumentException"><paramref name="data"/> 为空数组。</exception>
        public static IConfigTable<TKey, TRow> LoadTable<TKey, TRow>(byte[] data, Func<TRow, TKey> keySelector)
        {
            if (keySelector == null)
                throw new ArgumentNullException(nameof(keySelector));

            var rows = LoadFromBytes<TRow[]>(data);
            return new ConfigTable<TKey, TRow>(rows, keySelector);
        }
    }
}
