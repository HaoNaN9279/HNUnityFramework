using System;
using System.Collections.Generic;
using HN.Framework.Core.Level.Logic.Graph;
using UnityEditor;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 已注册节点类型的描述项。
    /// </summary>
    public sealed class GraphNodeTypeEntry
    {
        /// <summary>
        /// 初始化描述项。
        /// </summary>
        /// <param name="factory">节点工厂实例。</param>
        /// <param name="path">搜索菜单分类路径。</param>
        public GraphNodeTypeEntry(IGraphNodeFactory factory, string path)
        {
            Factory = factory;
            Path = path ?? string.Empty;
        }

        /// <summary>节点工厂实例。</summary>
        public IGraphNodeFactory Factory { get; }

        /// <summary>搜索菜单分类路径。</summary>
        public string Path { get; }

        /// <summary>节点类型标识。</summary>
        public string TypeId => Factory.TypeId;

        /// <summary>搜索菜单显示名称。</summary>
        public string DisplayName => Factory.DisplayName;

        /// <summary>分类路径首段（用于领域过滤）。</summary>
        public string Category
        {
            get
            {
                int separator = Path.IndexOf('/');
                return separator < 0 ? Path : Path.Substring(0, separator);
            }
        }
    }

    /// <summary>
    /// 节点类型注册中心。使用 <see cref="TypeCache"/> 扫描带 <see cref="GraphNodeTypeAttribute"/>
    /// 的 <see cref="IGraphNodeFactory"/> 实现并缓存。
    /// </summary>
    public static class GraphNodeRegistry
    {
        private static List<GraphNodeTypeEntry> _entries;

        /// <summary>全部已注册节点类型（按路径排序）。</summary>
        public static IReadOnlyList<GraphNodeTypeEntry> Entries
        {
            get
            {
                EnsureInitialized();
                return _entries;
            }
        }

        /// <summary>
        /// 按类型标识查找节点类型描述项。
        /// </summary>
        /// <param name="typeId">节点类型标识。</param>
        /// <returns>找到时返回描述项，否则返回 <c>null</c>。</returns>
        public static GraphNodeTypeEntry Find(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
            {
                return null;
            }

            EnsureInitialized();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].TypeId == typeId)
                {
                    return _entries[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 按领域分类获取节点类型描述项。分类为空时返回全部。
        /// </summary>
        /// <param name="category">分类名（路径首段）。</param>
        /// <returns>匹配的描述项列表。</returns>
        public static List<GraphNodeTypeEntry> GetEntries(string category)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(category))
            {
                return new List<GraphNodeTypeEntry>(_entries);
            }

            var result = new List<GraphNodeTypeEntry>();
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Category == category)
                {
                    result.Add(_entries[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// 使用指定类型标识创建节点数据。类型未注册时返回 <c>null</c>。
        /// </summary>
        /// <param name="typeId">节点类型标识。</param>
        /// <param name="id">节点唯一 Id。</param>
        /// <param name="position">节点位置。</param>
        /// <returns>新建节点，或 <c>null</c>。</returns>
        public static GraphNode CreateNode(string typeId, string id, GraphVector2 position)
        {
            var entry = Find(typeId);
            return entry?.Factory.Create(id, position);
        }

        /// <summary>清除缓存，下次访问时重新扫描。</summary>
        public static void Reload()
        {
            _entries = null;
        }

        private static void EnsureInitialized()
        {
            if (_entries != null)
            {
                return;
            }

            var list = new List<GraphNodeTypeEntry>();
            var types = TypeCache.GetTypesWithAttribute<GraphNodeTypeAttribute>();
            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                if (type.IsAbstract || type.IsInterface || !typeof(IGraphNodeFactory).IsAssignableFrom(type))
                {
                    continue;
                }

                var attribute = (GraphNodeTypeAttribute)Attribute.GetCustomAttribute(type, typeof(GraphNodeTypeAttribute));
                if (attribute == null)
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is IGraphNodeFactory factory)
                {
                    list.Add(new GraphNodeTypeEntry(factory, attribute.Path));
                }
            }

            list.Sort((a, b) =>
            {
                int byPath = string.CompareOrdinal(a.Path, b.Path);
                return byPath != 0 ? byPath : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });

            _entries = list;
        }
    }
}
