using System;

namespace HN.Framework.Core.Capability.Network
{
    /// <summary>
    /// 同步集合操作类型，描述对同步集合（List/Dictionary）执行的变更操作。
    /// 映射自 FishNet 的 <c>SyncListOperation</c> 枚举。
    /// </summary>
    public enum SyncCollectionOperation : byte
    {
        /// <summary>
        /// 向集合尾部添加一个元素。
        /// </summary>
        Add,

        /// <summary>
        /// 从集合中移除指定索引处的元素。
        /// </summary>
        Remove,

        /// <summary>
        /// 在集合的指定索引处插入一个元素。
        /// </summary>
        Insert,

        /// <summary>
        /// 更新集合中指定索引处的元素值。
        /// </summary>
        Set,

        /// <summary>
        /// 清空集合中的所有元素。
        /// </summary>
        Clear
    }

    /// <summary>
    /// 同步集合变更事件参数，描述对同步集合执行的一次变更操作。
    /// 用于将 FishNet SyncList/SyncDictionary 的变更桥接到框架事件系统。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public readonly struct SyncCollectionChange<T>
    {
        /// <summary>
        /// 获取变更操作类型。
        /// </summary>
        public SyncCollectionOperation Operation { get; }

        /// <summary>
        /// 获取变更涉及的索引位置。对于 Add 操作为集合 Count - 1，对于 Clear 操作为 -1。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 获取当前操作关联的元素（Add 的新元素、Remove/Insert/Set 的目标元素）。
        /// </summary>
        public T Item { get; }

        /// <summary>
        /// 获取替换前的旧元素。仅对 Set 操作有意义，其他操作返回 default(T)。
        /// </summary>
        public T OldItem { get; }

        /// <summary>
        /// 创建同步集合变更事件参数。
        /// </summary>
        /// <param name="operation">操作类型</param>
        /// <param name="index">索引位置</param>
        /// <param name="item">当前操作关联的元素</param>
        /// <param name="oldItem">替换前的旧元素，仅 Set 操作有意义</param>
        public SyncCollectionChange(SyncCollectionOperation operation, int index, T item, T oldItem = default)
        {
            Operation = operation;
            Index = index;
            Item = item;
            OldItem = oldItem;
        }
    }

    /// <summary>
    /// 同步字典变更事件参数，描述对同步字典执行的一次变更操作。
    /// 用于将 FishNet SyncDictionary 的变更桥接到框架事件系统。
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    public readonly struct SyncDictChange<TKey, TValue>
    {
        /// <summary>
        /// 获取变更操作类型。
        /// </summary>
        public SyncCollectionOperation Operation { get; }

        /// <summary>
        /// 获取变更涉及的键。
        /// </summary>
        public TKey Key { get; }

        /// <summary>
        /// 获取变更涉及的值。
        /// </summary>
        public TValue Value { get; }

        /// <summary>
        /// 创建同步字典变更事件参数。
        /// </summary>
        /// <param name="operation">操作类型</param>
        /// <param name="key">变更涉及的键</param>
        /// <param name="value">变更涉及的值</param>
        public SyncDictChange(SyncCollectionOperation operation, TKey key, TValue value)
        {
            Operation = operation;
            Key = key;
            Value = value;
        }
    }
}
