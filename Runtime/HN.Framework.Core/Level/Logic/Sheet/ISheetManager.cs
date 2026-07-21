#nullable enable

namespace HN.Framework.Core.Level.Logic.Sheet
{
    /// <summary>
    /// 配置表管理器接口，负责配置表的注册与查询。
    /// </summary>
    public interface ISheetManager
    {
        /// <summary>
        /// 根据表名获取已注册的配置表。
        /// </summary>
        /// <typeparam name="TKey">主键类型。</typeparam>
        /// <typeparam name="TRow">行数据类型。</typeparam>
        /// <param name="tableName">表名。</param>
        /// <returns>配置表实例。</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">指定表名未注册时抛出。</exception>
        IConfigTable<TKey, TRow> GetTable<TKey, TRow>(string tableName);

        /// <summary>
        /// 注册一个配置表到管理器。
        /// </summary>
        /// <typeparam name="TKey">主键类型。</typeparam>
        /// <typeparam name="TRow">行数据类型。</typeparam>
        /// <param name="tableName">表名。</param>
        /// <param name="table">配置表实例。</param>
        /// <exception cref="System.ArgumentException">表名已注册时抛出。</exception>
        void RegisterTable<TKey, TRow>(string tableName, IConfigTable<TKey, TRow> table);

        /// <summary>
        /// 检查指定表名是否已注册。
        /// </summary>
        /// <param name="tableName">表名。</param>
        /// <returns>已注册返回 true，否则 false。</returns>
        bool HasTable(string tableName);
    }
}
