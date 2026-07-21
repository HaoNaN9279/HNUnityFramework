#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace HN.Framework.Core.Level
{
    /// <summary>
    /// 查询节点类型。
    /// </summary>
    internal enum QueryNodeType : byte
    {
        /// <summary>叶子节点：匹配单个标签。</summary>
        Tag,

        /// <summary>复合节点：任意子节点匹配则为 true。</summary>
        Any,

        /// <summary>复合节点：所有子节点匹配则为 true。</summary>
        All,

        /// <summary>复合节点：对单个子节点取反。</summary>
        Not
    }

    /// <summary>
    /// 扁平节点（前序遍历表达式树中的节点）。
    /// </summary>
    internal struct FlatNode
    {
        /// <summary>节点类型。</summary>
        public QueryNodeType Type;

        /// <summary>仅在 <see cref="QueryNodeType.Tag"/> 类型时有效的标签。</summary>
        public GameplayTag Tag;

        /// <summary>
        /// 子节点数量。
        /// <see cref="QueryNodeType.Any"/> 和 <see cref="QueryNodeType.All"/> 的子节点数，
        /// <see cref="QueryNodeType.Not"/> 恒为 1，<see cref="QueryNodeType.Tag"/> 恒为 0。
        /// </summary>
        public int ChildCount;
    }

    /// <summary>
    /// GameplayTag 嵌套布尔查询表达式。
    /// 支持 Any/All/Not 组合查询，运行时常量时间评估，无虚调用、无字符串操作。
    /// 表达式树在构建时扁平化为数组，评估时纯数组遍历。
    /// </summary>
    public class TagQuery
    {
        private readonly FlatNode[] _nodes;
        private readonly int _rootStart;

        /// <summary>
        /// 初始化 TagQuery 实例（仅通过静态工厂方法构建）。
        /// </summary>
        /// <param name="nodes">前序遍历扁平化的表达式树节点数组。</param>
        /// <param name="rootStart">根节点在 nodes 数组中的起始索引。</param>
        private TagQuery(FlatNode[] nodes, int rootStart)
        {
            _nodes = nodes;
            _rootStart = rootStart;
        }

        /// <summary>
        /// 是否为空查询（无任何匹配条件，始终返回 true）。
        /// </summary>
        public bool IsEmpty => _nodes.Length == 0;

        /// <summary>
        /// 从单个标签创建一个查询。
        /// </summary>
        /// <param name="tag">要匹配的 GameplayTag。</param>
        /// <returns>仅匹配指定标签的查询表达式。</returns>
        public static TagQuery Make(GameplayTag tag)
        {
            var nodes = new FlatNode[1];
            nodes[0] = new FlatNode
            {
                Type = QueryNodeType.Tag,
                Tag = tag,
                ChildCount = 0
            };

            return new TagQuery(nodes, 0);
        }

        /// <summary>
        /// 创建 Any 查询：任意子查询匹配则整个查询匹配。
        /// </summary>
        /// <param name="queries">子查询列表。为 null 或空数组时返回空查询（始终匹配）。</param>
        /// <returns>Any 组合查询表达式。</returns>
        public static TagQuery Any(params TagQuery[] queries)
        {
            if (queries == null || queries.Length == 0)
            {
                return new TagQuery(Array.Empty<FlatNode>(), 0);
            }

            // 过滤掉空查询（空查询不产生任何节点，不应计入 ChildCount）
            var nonEmpty = new List<TagQuery>(queries.Length);
            for (int i = 0; i < queries.Length; i++)
            {
                var q = queries[i];
                if (q != null && q._nodes.Length > 0)
                {
                    nonEmpty.Add(q);
                }
            }

            if (nonEmpty.Count == 0)
            {
                return new TagQuery(Array.Empty<FlatNode>(), 0);
            }

            var allNodes = new List<FlatNode>();
            allNodes.Add(new FlatNode
            {
                Type = QueryNodeType.Any,
                ChildCount = nonEmpty.Count
            });

            for (int i = 0; i < nonEmpty.Count; i++)
            {
                allNodes.AddRange(nonEmpty[i]._nodes);
            }

            return new TagQuery(allNodes.ToArray(), 0);
        }

        /// <summary>
        /// 创建 All 查询：所有子查询都匹配则整个查询匹配。
        /// </summary>
        /// <param name="queries">子查询列表。为 null 或空数组时返回空查询（始终匹配）。</param>
        /// <returns>All 组合查询表达式。</returns>
        public static TagQuery All(params TagQuery[] queries)
        {
            if (queries == null || queries.Length == 0)
            {
                return new TagQuery(Array.Empty<FlatNode>(), 0);
            }

            // 过滤掉空查询
            var nonEmpty = new List<TagQuery>(queries.Length);
            for (int i = 0; i < queries.Length; i++)
            {
                var q = queries[i];
                if (q != null && q._nodes.Length > 0)
                {
                    nonEmpty.Add(q);
                }
            }

            if (nonEmpty.Count == 0)
            {
                return new TagQuery(Array.Empty<FlatNode>(), 0);
            }

            var allNodes = new List<FlatNode>();
            allNodes.Add(new FlatNode
            {
                Type = QueryNodeType.All,
                ChildCount = nonEmpty.Count
            });

            for (int i = 0; i < nonEmpty.Count; i++)
            {
                allNodes.AddRange(nonEmpty[i]._nodes);
            }

            return new TagQuery(allNodes.ToArray(), 0);
        }

        /// <summary>
        /// 创建 Not 查询：对单个子查询取反。
        /// </summary>
        /// <param name="query">要取反的子查询。</param>
        /// <returns>Not 查询表达式。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 null。</exception>
        public static TagQuery Not(TagQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            // Not(空查询) = 始终 false，构建为无子节点的 Any（循环直接跳过，返回 false）
            if (query._nodes.Length == 0)
            {
                var falseNodes = new FlatNode[1];
                falseNodes[0] = new FlatNode
                {
                    Type = QueryNodeType.Any,
                    ChildCount = 0
                };

                return new TagQuery(falseNodes, 0);
            }

            var allNodes = new List<FlatNode>(1 + query._nodes.Length);
            allNodes.Add(new FlatNode
            {
                Type = QueryNodeType.Not,
                ChildCount = 1
            });
            allNodes.AddRange(query._nodes);

            return new TagQuery(allNodes.ToArray(), 0);
        }

        /// <summary>
        /// 针对容器评估查询表达式。
        /// </summary>
        /// <param name="container">要评估的 GameplayTagContainer。</param>
        /// <returns>如果查询表达式匹配容器中的标签，则返回 <c>true</c>。</returns>
        public bool Matches(GameplayTagContainer container)
        {
            if (_nodes.Length == 0)
            {
                return true;
            }

            int index = _rootStart;
            return EvaluateNode(container, ref index);
        }

        /// <summary>
        /// 递归评估扁平数组中的节点。
        /// </summary>
        /// <param name="container">要评估的标签容器。</param>
        /// <param name="index">当前节点索引（前序遍历中递增）。</param>
        /// <returns>当前节点的评估结果。</returns>
        private bool EvaluateNode(GameplayTagContainer container, ref int index)
        {
            var node = _nodes[index];
            index++;

            switch (node.Type)
            {
                case QueryNodeType.Tag:
                    return container.HasTag(node.Tag);

                case QueryNodeType.Any:
                    for (int i = 0; i < node.ChildCount; i++)
                    {
                        if (EvaluateNode(container, ref index))
                        {
                            // 跳过剩余未评估的子节点，保持索引正确
                            SkipRemainingSiblings(ref index, node.ChildCount - i - 1);
                            return true;
                        }
                    }

                    return false;

                case QueryNodeType.All:
                    for (int i = 0; i < node.ChildCount; i++)
                    {
                        if (!EvaluateNode(container, ref index))
                        {
                            // 跳过剩余未评估的子节点
                            SkipRemainingSiblings(ref index, node.ChildCount - i - 1);
                            return false;
                        }
                    }

                    return true;

                case QueryNodeType.Not:
                    return !EvaluateNode(container, ref index);

                default:
                    return false;
            }
        }

        /// <summary>
        /// 跳过指定数量的直接子节点（前序遍历中推进索引）。
        /// 当 Any 短路为 true 或 All 短路为 false 时调用，跳过尚未评估的子节点。
        /// </summary>
        /// <param name="index">当前索引引用。</param>
        /// <param name="count">需要跳过的子节点数量。</param>
        private void SkipRemainingSiblings(ref int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                SkipSubtree(ref index);
            }
        }

        /// <summary>
        /// 跳过以当前索引为根的整个子树（前序遍历中推进索引）。
        /// </summary>
        /// <param name="index">当前索引引用（指向子树的根节点）。</param>
        private void SkipSubtree(ref int index)
        {
            var node = _nodes[index];
            index++;

            for (int i = 0; i < node.ChildCount; i++)
            {
                SkipSubtree(ref index);
            }
        }

        /// <summary>
        /// 返回表达式字符串表示（调试用）。
        /// </summary>
        /// <returns>查询表达式的可读字符串。</returns>
        public override string ToString()
        {
            if (_nodes.Length == 0)
            {
                return "(Empty)";
            }

            var sb = new StringBuilder();
            int index = _rootStart;
            AppendNodeToString(sb, ref index);
            return sb.ToString();
        }

        /// <summary>
        /// 递归追加节点字符串表示到 StringBuilder。
        /// </summary>
        private void AppendNodeToString(StringBuilder sb, ref int index)
        {
            var node = _nodes[index];
            index++;

            switch (node.Type)
            {
                case QueryNodeType.Tag:
                    var tagName = GameplayTagManager.GetTagName(node.Tag);
                    sb.Append(tagName ?? $"Tag({node.Tag.TableIndex})");
                    break;

                case QueryNodeType.Any:
                    sb.Append("Any(");
                    for (int i = 0; i < node.ChildCount; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }

                        AppendNodeToString(sb, ref index);
                    }

                    sb.Append(')');
                    break;

                case QueryNodeType.All:
                    sb.Append("All(");
                    for (int i = 0; i < node.ChildCount; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }

                        AppendNodeToString(sb, ref index);
                    }

                    sb.Append(')');
                    break;

                case QueryNodeType.Not:
                    sb.Append("Not(");
                    AppendNodeToString(sb, ref index);
                    sb.Append(')');
                    break;
            }
        }

        /// <summary>
        /// 获取序列化用的内部节点数组（仅供序列化器使用）。
        /// </summary>
        internal FlatNode[] GetNodesForSerialization() => _nodes;

        /// <summary>
        /// 从扁平节点数组重建 TagQuery 实例（仅供反序列化器使用）。
        /// </summary>
        /// <param name="nodes">前序遍历扁平化的表达式树节点数组。</param>
        /// <returns>重建的 TagQuery 实例。</returns>
        internal static TagQuery CreateFromNodes(FlatNode[] nodes)
        {
            return new TagQuery(nodes, 0);
        }
    }
}
