#nullable enable

using System;
using System.Collections.Generic;
using HN.Framework.Core.Capability.UI;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// 红点树运行时管理器，提供基于路径的红点节点注册、计数与订阅功能。
    /// 路径用于组织节点层级，如 "Mail/System/Unread" 表示三层嵌套节点。
    /// </summary>
    public class RedDotManager : IDisposable
    {
        private readonly Dictionary<string, RedDotNode> _nodes = new();
        private readonly Dictionary<string, List<Action<int>>> _subscriptions = new();

        /// <summary>
        /// 注册一个红点路径，自动创建路径上所需的中间节点。
        /// 若路径已存在，直接返回现有节点。
        /// </summary>
        /// <param name="path">红点路径，以 '/' 分隔（如 "Mail/System/Unread"）。</param>
        /// <returns>路径末端对应的 RedDotNode。</returns>
        /// <exception cref="ArgumentNullException">path 为 null 时抛出。</exception>
        public RedDotNode Register(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (_nodes.TryGetValue(path, out var existing))
                return existing;

            var segments = SplitPath(path);
            if (segments.Length == 0)
                throw new ArgumentException("Path must contain at least one non-empty segment.", nameof(path));

            RedDotNode? parent = null;
            var currentPath = string.Empty;

            for (int i = 0; i < segments.Length; i++)
            {
                currentPath = i == 0 ? segments[i] : currentPath + "/" + segments[i];

                if (!_nodes.TryGetValue(currentPath, out var node))
                {
                    node = new RedDotNode(segments[i]);
                    _nodes[currentPath] = node;

                    if (parent != null)
                    {
                        parent.AddChild(node);
                    }
                }

                parent = node;
            }

            return parent!;
        }

        /// <summary>
        /// 注销指定路径的红点节点及其所有子孙节点。
        /// 同时取消所有受影响路径上的订阅回调。
        /// </summary>
        /// <param name="path">要注销的红点路径。</param>
        /// <returns>如果成功移除则返回 true；路径不存在时返回 false。</returns>
        public bool Unregister(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (!_nodes.TryGetValue(path, out var node))
                return false;

            // 收集所有需移除的路径（自身 + 所有以 path/ 开头的子孙）
            var pathsToRemove = GetDescendantPaths(path);

            // 取消所有受影响路径的订阅
            foreach (var p in pathsToRemove)
            {
                if (_subscriptions.TryGetValue(p, out var callbacks))
                {
                    if (_nodes.TryGetValue(p, out var targetNode))
                    {
                        foreach (var cb in callbacks)
                        {
                            targetNode.OnCountChanged -= cb;
                        }
                    }

                    _subscriptions.Remove(p);
                }

                _nodes.Remove(p);
            }

            // 从父节点断开
            if (node.Parent != null)
            {
                node.Parent.RemoveChild(node);
            }

            return true;
        }

        /// <summary>
        /// 查找指定路径的红点节点。
        /// </summary>
        /// <param name="path">红点路径。</param>
        /// <returns>若找到则返回节点，否则返回 null。</returns>
        public RedDotNode? GetNode(string path)
        {
            if (path == null)
                return null;

            _nodes.TryGetValue(path, out var node);
            return node;
        }

        /// <summary>
        /// 设置指定路径节点的直接计数值。
        /// 若路径不存在，此方法无操作。
        /// </summary>
        /// <param name="path">红点路径。</param>
        /// <param name="count">要设置的计数值。</param>
        public void SetCount(string path, int count)
        {
            if (path == null)
                return;

            if (_nodes.TryGetValue(path, out var node))
            {
                node.SetCount(count);
            }
        }

        /// <summary>
        /// 获取指定路径节点的有效计数值（自身 + 所有子节点）。
        /// </summary>
        /// <param name="path">红点路径。</param>
        /// <returns>有效计数值；路径不存在时返回 0。</returns>
        public int GetCount(string path)
        {
            if (path == null)
                return 0;

            if (_nodes.TryGetValue(path, out var node))
            {
                return node.Count;
            }

            return 0;
        }

        /// <summary>
        /// 订阅指定路径节点的计数变更。
        /// 当该节点的有效 Count 变化时触发回调。
        /// </summary>
        /// <param name="path">红点路径。</param>
        /// <param name="onCountChanged">计数变更回调，参数为新的有效 Count 值。</param>
        public void Subscribe(string path, Action<int> onCountChanged)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var node = GetNode(path);
            if (node == null)
                return;

            if (!_subscriptions.ContainsKey(path))
            {
                _subscriptions[path] = new List<Action<int>>();
            }

            _subscriptions[path].Add(onCountChanged);
            node.OnCountChanged += onCountChanged;
        }

        /// <summary>
        /// 取消订阅指定路径节点的计数变更。
        /// </summary>
        /// <param name="path">红点路径。</param>
        /// <param name="onCountChanged">要移除的计数变更回调。</param>
        public void Unsubscribe(string path, Action<int> onCountChanged)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            var node = GetNode(path);
            if (node == null)
                return;

            if (_subscriptions.TryGetValue(path, out var list))
            {
                list.Remove(onCountChanged);
                if (list.Count == 0)
                {
                    _subscriptions.Remove(path);
                }
            }

            node.OnCountChanged -= onCountChanged;
        }

        /// <summary>
        /// 清空所有注册的红点节点及订阅回调。
        /// </summary>
        public void Clear()
        {
            // 取消所有订阅
            foreach (var kv in _subscriptions)
            {
                if (_nodes.TryGetValue(kv.Key, out var node))
                {
                    foreach (var cb in kv.Value)
                    {
                        node.OnCountChanged -= cb;
                    }
                }
            }

            _subscriptions.Clear();
            _nodes.Clear();
        }

        /// <summary>
        /// 释放资源，清空所有节点和订阅。
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// 将路径按 '/' 分割，并过滤空段。
        /// </summary>
        private string[] SplitPath(string path)
        {
            return path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 收集指定路径及其所有子孙节点的路径。
        /// </summary>
        private List<string> GetDescendantPaths(string path)
        {
            var result = new List<string> { path };
            var prefix = path + "/";

            foreach (var key in _nodes.Keys)
            {
                if (key.StartsWith(prefix, StringComparison.Ordinal))
                {
                    result.Add(key);
                }
            }

            return result;
        }
    }
}
