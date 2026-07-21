#nullable enable

using System;

namespace HN.Framework.Core.Level.Logic.Inventory
{
    /// <summary>
    /// 容器默认实现。使用定长数组管理槽位，支持堆叠和事件通知。
    /// </summary>
    public sealed class Container : IContainer
    {
        public ContainerType Type { get; }
        public int Capacity { get; }
        public int SlotCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Capacity; i++)
                    if (!_slots[i].IsEmpty) count++;
                return count;
            }
        }

        private readonly ContainerSlot[] _slots;
        private readonly int _containerIndex; // 用于事件追踪

        public event Action<ItemAddedEvent>? OnItemAdded;
        public event Action<ItemRemovedEvent>? OnItemRemoved;
        public event Action<ItemStackChangedEvent>? OnStackChanged;
        public event Action<ItemMovedEvent>? OnItemMoved;
        public event Action<ContainerClearedEvent>? OnCleared;

        public Container(ContainerType type, int capacity = 20, int containerIndex = 0)
        {
            Type = type;
            Capacity = capacity > 0 ? capacity : 20;
            _slots = new ContainerSlot[Capacity];
            _containerIndex = containerIndex;
        }

        public ContainerSlot GetSlot(int index)
        {
            ValidateIndex(index);
            return _slots[index];
        }

        public bool TryAddItem(ItemInstance item)
        {
            if (item == null) return false;

            // 先尝试堆叠到已有的同种物品
            for (int i = 0; i < Capacity; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].Item!.CanStackWith(item))
                {
                    return TryStackAt(i, item);
                }
            }

            // 找空槽位
            for (int i = 0; i < Capacity; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    return TryPlaceAt(i, item);
                }
            }

            return false; // 容器满
        }

        public bool TryAddItemAt(int index, ItemInstance item)
        {
            ValidateIndex(index);
            if (item == null) return false;

            if (!_slots[index].IsEmpty)
            {
                // 尝试堆叠
                if (_slots[index].Item!.CanStackWith(item))
                {
                    return TryStackAt(index, item);
                }
                return false; // 槽位被占用且不能堆叠
            }

            return TryPlaceAt(index, item);
        }

        public bool RemoveItem(int index, int count)
        {
            ValidateIndex(index);
            if (count <= 0 || _slots[index].IsEmpty) return false;
            if (_slots[index].StackCount < count) return false;

            int oldCount = _slots[index].StackCount;
            int newCount = oldCount - count;

            if (newCount <= 0)
            {
                _slots[index].Clear();
                OnItemRemoved?.Invoke(new ItemRemovedEvent(_containerIndex, index, _slots[index].Item?.ItemDefId ?? 0, count));
            }
            else
            {
                _slots[index].StackCount = newCount;
                OnStackChanged?.Invoke(new ItemStackChangedEvent(_containerIndex, index, oldCount, newCount));
            }

            return true;
        }

        public bool RemoveItemAt(int index)
        {
            ValidateIndex(index);
            if (_slots[index].IsEmpty) return false;

            int itemDefId = _slots[index].Item?.ItemDefId ?? 0;
            int count = _slots[index].StackCount;

            if (_slots[index].Item != null)
            {
                _slots[index].Item.SlotIndex = -1;
            }

            _slots[index].Clear();
            OnItemRemoved?.Invoke(new ItemRemovedEvent(_containerIndex, index, itemDefId, count));
            return true;
        }

        public void SwapSlots(int indexA, int indexB)
        {
            ValidateIndex(indexA);
            ValidateIndex(indexB);
            if (indexA == indexB) return;

            (_slots[indexA], _slots[indexB]) = (_slots[indexB], _slots[indexA]);

            // 更新 ItemInstance 内部的 SlotIndex
            if (_slots[indexA].Item != null) _slots[indexA].Item.SlotIndex = indexA;
            if (_slots[indexB].Item != null) _slots[indexB].Item.SlotIndex = indexB;

            OnItemMoved?.Invoke(new ItemMovedEvent(_containerIndex, indexA, indexB));
        }

        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (_slots[i].Item != null)
                {
                    _slots[i].Item.SlotIndex = -1;
                }
                _slots[i].Clear();
            }
            OnCleared?.Invoke(new ContainerClearedEvent(_containerIndex));
        }

        public int GetItemCount(int itemDefId)
        {
            int total = 0;
            for (int i = 0; i < Capacity; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].Item!.ItemDefId == itemDefId)
                {
                    total += _slots[i].StackCount;
                }
            }
            return total;
        }

        public bool HasItem(int itemDefId, int count = 1)
        {
            return GetItemCount(itemDefId) >= count;
        }

        private bool TryStackAt(int index, ItemInstance item)
        {
            int oldCount = _slots[index].StackCount;
            _slots[index].StackCount += item.StackCount;
            OnStackChanged?.Invoke(new ItemStackChangedEvent(_containerIndex, index, oldCount, _slots[index].StackCount));
            return true;
        }

        private bool TryPlaceAt(int index, ItemInstance item)
        {
            item.SlotIndex = index;
            _slots[index].Item = item;
            _slots[index].StackCount = item.StackCount;
            OnItemAdded?.Invoke(new ItemAddedEvent(_containerIndex, index, item));
            return true;
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= Capacity)
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} out of range [0, {Capacity})");
        }
    }
}
