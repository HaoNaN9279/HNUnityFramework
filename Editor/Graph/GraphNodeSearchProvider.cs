using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 节点搜索提供器。从 <see cref="GraphNodeRegistry"/> 生成搜索树，
    /// 并在选中时请求编辑器视图创建节点。
    /// </summary>
    public sealed class GraphNodeSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private GraphEditorView _view;

        /// <summary>分类过滤（节点类型路径首段）。为空时展示全部节点类型。</summary>
        public string CategoryFilter { get; set; }

        /// <summary>
        /// 绑定目标编辑器视图。
        /// </summary>
        /// <param name="view">编辑器视图。</param>
        public void Initialize(GraphEditorView view)
        {
            _view = view;
            if (view != null)
            {
                CategoryFilter = view.CategoryFilter;
            }
        }

        /// <inheritdoc />
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Nodes"), 0)
            };

            var entries = GraphNodeRegistry.GetEntries(CategoryFilter);
            var groups = new Dictionary<string, List<GraphNodeTypeEntry>>();
            var order = new List<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!groups.TryGetValue(entry.Path, out var list))
                {
                    list = new List<GraphNodeTypeEntry>();
                    groups.Add(entry.Path, list);
                    order.Add(entry.Path);
                }

                list.Add(entry);
            }

            for (int i = 0; i < order.Count; i++)
            {
                var path = order[i];
                tree.Add(new SearchTreeGroupEntry(new GUIContent(path), 1));
                var list = groups[path];
                for (int j = 0; j < list.Count; j++)
                {
                    tree.Add(new SearchTreeEntry(new GUIContent(list[j].DisplayName))
                    {
                        level = 2,
                        userData = list[j].Factory
                    });
                }
            }

            return tree;
        }

        /// <inheritdoc />
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (_view == null || !(searchTreeEntry.userData is IGraphNodeFactory factory))
            {
                return false;
            }

            _view.CreateNodeFromFactory(factory, context.screenMousePosition);
            return true;
        }
    }
}
