using System;
using FishNet.Object.Synchronizing;
using HN.Framework.Core.Capability.Network;

namespace HN.Framework.Unity.Capability.Network
{
    /// <summary>
    /// 框架级的同步列表包装器，继承自 FishNet <see cref="SyncList{T}"/>。
    /// 提供框架统一的 <see cref="OnCollectionChanged"/> 事件，将 FishNet 的 <c>SyncListOperation</c>
    /// 映射为框架的 <see cref="SyncCollectionOperation"/>，便于 View 层监听。
    /// </summary>
    /// <typeparam name="T">列表元素类型</typeparam>
    [System.Serializable]
    public class FishNetSyncedList<T> : SyncList<T>
    {
        /// <summary>
        /// 集合发生变化时触发，参数为框架统一的 <see cref="SyncCollectionChange{T}"/> 事件数据。
        /// </summary>
        public event Action<SyncCollectionChange<T>> OnCollectionChanged;

        /// <summary>
        /// 创建同步列表包装器实例。
        /// </summary>
        public FishNetSyncedList()
        {
            OnChange += HandleOnChange;
        }

        private void HandleOnChange(SyncListOperation op, int index, T oldItem, T newItem, bool asServer)
        {
            SyncCollectionOperation frameworkOp = MapOperation(op);
            T item = (frameworkOp == SyncCollectionOperation.Remove) ? oldItem : newItem;
            T old = (frameworkOp == SyncCollectionOperation.Set) ? oldItem : default;

            SyncCollectionChange<T> change = new SyncCollectionChange<T>(frameworkOp, index, item, old);
            OnCollectionChanged?.Invoke(change);
        }

        private static SyncCollectionOperation MapOperation(SyncListOperation op)
        {
            switch (op)
            {
                case SyncListOperation.Add: return SyncCollectionOperation.Add;
                case SyncListOperation.RemoveAt: return SyncCollectionOperation.Remove;
                case SyncListOperation.Insert: return SyncCollectionOperation.Insert;
                case SyncListOperation.Set: return SyncCollectionOperation.Set;
                case SyncListOperation.Clear: return SyncCollectionOperation.Clear;
                default: return SyncCollectionOperation.Add;
            }
        }
    }
}
