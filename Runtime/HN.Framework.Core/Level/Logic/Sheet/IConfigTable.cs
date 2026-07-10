#nullable enable

using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// 配置表泛型接口，提供主键查询、遍历与存在性检查。
    /// </summary>
    /// <typeparam name="TKey">主键类型。</typeparam>
    /// <typeparam name="TRow">行数据类型。</typeparam>
    public interface IConfigTable<TKey, TRow>
    {
        /// <summary>
        /// 根据主键获取对应的行数据。
        /// </summary>
        /// <param name="key">主键值。</param>
        /// <returns>行数据。</returns>
        /// <exception cref="KeyNotFoundException">指定主键不存在时抛出。</exception>
        TRow Get(TKey key);

        /// <summary>
        /// 尝试根据主键获取对应的行数据。
        /// </summary>
        /// <param name="key">主键值。</param>
        /// <param name="row">获取成功时输出行数据，否则为默认值。</param>
        /// <returns>主键存在返回 true，否则 false。</returns>
        bool TryGet(TKey key, out TRow row);

        /// <summary>
        /// 获取表中所有行数据数组。
        /// </summary>
        /// <returns>行数据数组，按构造时传入顺序排列。</returns>
        TRow[] GetAll();

        /// <summary>
        /// 表中行数据的总数。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 判断指定主键是否存在于表中。
        /// </summary>
        /// <param name="key">主键值。</param>
        /// <returns>存在返回 true，否则 false。</returns>
        bool ContainsKey(TKey key);
    }
}
