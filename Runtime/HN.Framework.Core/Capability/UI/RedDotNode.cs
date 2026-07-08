#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// 红点树节点，支持树状聚合计数与父子节点变更事件。
    /// 每个节点的有效 Count = 自身 SetCount 设置的值 + 所有子节点的有效 Count 之和。
    /// </summary>
    public class RedDotNode
    {
        private readonly string _key;
        private int _directCount;
        private int _effectiveCount;
        private readonly List<RedDotNode> _children;
        private readonly ReadOnlyCollection<RedDotNode> _childrenReadOnly;

        /// <summary>
        /// 节点的唯一标识键。
        /// </summary>
        public string Key => _key;

        /// <summary>
        /// 节点的有效计数值（自身 Count + 所有子节点 Count 之和）。
        /// </summary>
        public int Count => _effectiveCount;

        /// <summary>
        /// 父节点，根节点的 Parent 为 null。
        /// </summary>
        public RedDotNode? Parent { get; private set; }

        /// <summary>
        /// 子节点只读集合。
        /// </summary>
        public IReadOnlyList<RedDotNode> Children => _childrenReadOnly;

        /// <summary>
        /// 当节点的有效 Count 发生变化时触发，参数为新的 Count 值。
        /// </summary>
        public event Action<int>? OnCountChanged;

        /// <summary>
        /// 创建红点树节点。
        /// </summary>
        /// <param name="key">节点唯一标识键。</param>
        /// <exception cref="ArgumentNullException">key 为 null 时抛出。</exception>
        public RedDotNode(string key)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _children = new List<RedDotNode>();
            _childrenReadOnly = new ReadOnlyCollection<RedDotNode>(_children);
        }

        /// <summary>
        /// 创建带初始计数的红点树节点。
        /// </summary>
        /// <param name="key">节点唯一标识键。</param>
        /// <param name="initialCount">初始计数值。</param>
        /// <exception cref="ArgumentNullException">key 为 null 时抛出。</exception>
        public RedDotNode(string key, int initialCount)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _directCount = initialCount;
            _effectiveCount = initialCount;
            _children = new List<RedDotNode>();
            _childrenReadOnly = new ReadOnlyCollection<RedDotNode>(_children);
        }

        /// <summary>
        /// 设置节点的直接计数值，并触发父节点向上聚合重算。
        /// </summary>
        /// <param name="count">新的计数值。</param>
        public void SetCount(int count)
        {
            _directCount = count;
            RecalculateEffectiveCount();
        }

        /// <summary>
        /// 添加子节点，子节点的 Count 变更会自动聚合到当前节点。
        /// </summary>
        /// <param name="child">要添加的子节点。</param>
        /// <exception cref="ArgumentNullException">child 为 null 时抛出。</exception>
        /// <exception cref="InvalidOperationException">child 已有父节点或 child 是当前节点的祖先时抛出。</exception>
        public void AddChild(RedDotNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (child.Parent != null)
                throw new InvalidOperationException(
                    $"子节点 '{child.Key}' 已经挂载在父节点 '{child.Parent.Key}' 下，请先从原父节点移除后再添加。");

            if (IsDescendantOf(child))
                throw new InvalidOperationException(
                    $"不能将祖先节点 '{child.Key}' 设为子节点，这会导致循环引用。");

            _children.Add(child);
            child.Parent = this;
            RecalculateEffectiveCount();
        }

        /// <summary>
        /// 移除子节点，该子节点的 Count 不再聚合到当前节点。
        /// </summary>
        /// <param name="child">要移除的子节点。</param>
        /// <returns>如果成功移除则返回 true；如果 child 不在此节点的子列表中则返回 false。</returns>
        /// <exception cref="ArgumentNullException">child 为 null 时抛出。</exception>
        public bool RemoveChild(RedDotNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            if (_children.Remove(child))
            {
                child.Parent = null;
                RecalculateEffectiveCount();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重新计算有效 Count 并向上传播。
        /// </summary>
        private void RecalculateEffectiveCount()
        {
            int newCount = _directCount;

            // 聚合所有子节点的有效 Count
            for (int i = 0; i < _children.Count; i++)
            {
                newCount += _children[i]._effectiveCount;
            }

            if (newCount != _effectiveCount)
            {
                _effectiveCount = newCount;
                OnCountChanged?.Invoke(_effectiveCount);
                Parent?.RecalculateEffectiveCount();
            }
        }

        /// <summary>
        /// 检查当前节点是否是 potentialAncestor 的后代节点。
        /// </summary>
        private bool IsDescendantOf(RedDotNode potentialAncestor)
        {
            RedDotNode? current = this;
            while (current != null)
            {
                if (current == potentialAncestor)
                    return true;
                current = current.Parent;
            }
            return false;
        }
    }
}
