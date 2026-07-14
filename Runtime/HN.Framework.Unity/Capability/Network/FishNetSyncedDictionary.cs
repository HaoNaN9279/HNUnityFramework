using System;
using FishNet.Object.Synchronizing;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 框架级的同步字典包装器，继承自 FishNet <see cref="SyncDictionary{TKey, TValue}"/>。
    /// 提供框架统一的 <see cref="OnCollectionChanged"/> 事件，将 FishNet 的 <c>SyncDictionaryOperation</c>
    /// 映射为框架的 <see cref="SyncCollectionOperation"/>。
    /// </summary>
    /// <typeparam name="TKey">字典键类型</typeparam>
    /// <typeparam name="TValue">字典值类型</typeparam>
    [System.Serializable]
    public class FishNetSyncedDictionary<TKey, TValue> : SyncDictionary<TKey, TValue>
    {
        /// <summary>
        /// 集合发生变化时触发，参数为框架统一的 <see cref="SyncDictChange{TKey, TValue}"/> 事件数据。
        /// </summary>
        public event Action<SyncDictChange<TKey, TValue>> OnCollectionChanged;

        /// <summary>
        /// 创建同步字典包装器实例。
        /// </summary>
        public FishNetSyncedDictionary()
        {
            OnChange += HandleOnChange;
        }

        private void HandleOnChange(SyncDictionaryOperation op, TKey key, TValue value, bool asServer)
        {
            SyncCollectionOperation frameworkOp = MapOperation(op);
            SyncDictChange<TKey, TValue> change = new SyncDictChange<TKey, TValue>(frameworkOp, key, value);
            OnCollectionChanged?.Invoke(change);
        }

        private static SyncCollectionOperation MapOperation(SyncDictionaryOperation op)
        {
            switch (op)
            {
                case SyncDictionaryOperation.Add: return SyncCollectionOperation.Add;
                case SyncDictionaryOperation.Remove: return SyncCollectionOperation.Remove;
                case SyncDictionaryOperation.Set: return SyncCollectionOperation.Set;
                case SyncDictionaryOperation.Clear: return SyncCollectionOperation.Clear;
                default: return SyncCollectionOperation.Add;
            }
        }
    }
}
