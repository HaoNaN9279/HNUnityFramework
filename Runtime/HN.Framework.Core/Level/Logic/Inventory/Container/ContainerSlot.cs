#nullable enable

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 容器槽位数据。
    /// </summary>
    public struct ContainerSlot
    {
        /// <summary>槽位中的物品实例（null 表示空）。</summary>
        public ItemInstance? Item;

        /// <summary>当前堆叠数。</summary>
        public int StackCount;

        /// <summary>该槽位是否为空。</summary>
        public readonly bool IsEmpty => Item == null || StackCount <= 0;

        /// <summary>清空该槽位。</summary>
        public void Clear()
        {
            Item = null;
            StackCount = 0;
        }
    }
}
