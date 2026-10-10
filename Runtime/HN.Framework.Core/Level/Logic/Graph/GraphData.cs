#nullable enable

using MemoryPack;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 通用图数据模型（节点 + 边 + 编组），纯数据、MemoryPack 可序列化。
    /// 作为动画状态机图（<c>AnimationGraph</c>）、行为树图、对话图等领域图的基类。
    /// 不包含运行时求值逻辑，运行时执行由各领域自行实现。
    /// </summary>
    [MemoryPackable]
    public partial class GraphData
    {
        /// <summary>图唯一标识。</summary>
        [MemoryPackOrder(0)]
        public string Id { get; set; }

        /// <summary>图显示名称。</summary>
        [MemoryPackOrder(1)]
        public string Name { get; set; }

        /// <summary>节点集合。</summary>
        [MemoryPackOrder(2)]
        public List<GraphNode> Nodes { get; set; }

        /// <summary>边集合。</summary>
        [MemoryPackOrder(3)]
        public List<GraphEdge> Edges { get; set; }

        /// <summary>编组集合。</summary>
        [MemoryPackOrder(4)]
        public List<GraphGroup> Groups { get; set; }

        /// <summary>节点数量。</summary>
        public int NodeCount => Nodes.Count;

        /// <summary>边数量。</summary>
        public int EdgeCount => Edges.Count;

        /// <summary>编组数量。</summary>
        public int GroupCount => Groups.Count;

        /// <summary>
        /// 初始化空图。
        /// </summary>
        public GraphData()
        {
            Id = string.Empty;
            Name = string.Empty;
            Nodes = new List<GraphNode>();
            Edges = new List<GraphEdge>();
            Groups = new List<GraphGroup>();
        }

        /// <summary>
        /// 初始化图。
        /// </summary>
        /// <param name="id">图唯一标识。</param>
        /// <param name="name">图显示名称。</param>
        public GraphData(string id, string name = "")
            : this()
        {
            Id = id;
            Name = name;
        }

        // -------- 节点 --------

        /// <summary>
        /// 按 Id 查找节点。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>找到时返回节点，否则返回 <c>null</c>。</returns>
        public GraphNode? GetNode(string nodeId)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Id == nodeId)
                {
                    return Nodes[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 是否存在指定 Id 的节点。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>存在返回 <c>true</c>。</returns>
        public bool ContainsNode(string nodeId) => GetNode(nodeId) != null;

        /// <summary>
        /// 添加节点。Id 为空或已存在时失败。
        /// </summary>
        /// <param name="node">要添加的节点。</param>
        /// <returns>新增返回 <c>true</c>。</returns>
        public bool AddNode(GraphNode node)
        {
            if (node == null || string.IsNullOrEmpty(node.Id) || ContainsNode(node.Id))
            {
                return false;
            }

            Nodes.Add(node);
            return true;
        }

        /// <summary>
        /// 移除节点，同时移除与之相连的所有边，并从所有编组中移除该节点。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>移除成功返回 <c>true</c>；节点不存在返回 <c>false</c>。</returns>
        public bool RemoveNode(string nodeId)
        {
            int index = IndexOfNode(nodeId);
            if (index < 0)
            {
                return false;
            }

            Nodes.RemoveAt(index);
            RemoveEdgesConnectingNode(nodeId);

            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].RemoveNode(nodeId);
            }

            return true;
        }

        // -------- 边 --------

        /// <summary>
        /// 按 Id 查找边。
        /// </summary>
        /// <param name="edgeId">边 Id。</param>
        /// <returns>找到时返回边，否则返回 <c>null</c>。</returns>
        public GraphEdge? GetEdge(string edgeId)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Id == edgeId)
                {
                    return Edges[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 添加边。边为 null、Id 为空或已存在时失败。
        /// 不校验端点是否存在，需调用 <see cref="Validate"/> 进行一致性检查。
        /// </summary>
        /// <param name="edge">要添加的边。</param>
        /// <returns>新增返回 <c>true</c>。</returns>
        public bool AddEdge(GraphEdge edge)
        {
            if (edge == null || string.IsNullOrEmpty(edge.Id) || GetEdge(edge.Id) != null)
            {
                return false;
            }

            Edges.Add(edge);
            return true;
        }

        /// <summary>
        /// 移除边。
        /// </summary>
        /// <param name="edgeId">边 Id。</param>
        /// <returns>移除成功返回 <c>true</c>。</returns>
        public bool RemoveEdge(string edgeId)
        {
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].Id == edgeId)
                {
                    Edges.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取指定节点作为源的所有出边。
        /// </summary>
        /// <param name="nodeId">源节点 Id。</param>
        /// <returns>出边集合（新建列表）。</returns>
        public List<GraphEdge> GetEdgesFromNode(string nodeId)
        {
            var result = new List<GraphEdge>();
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].SourceNodeId == nodeId)
                {
                    result.Add(Edges[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取指定节点作为目标的所有入边。
        /// </summary>
        /// <param name="nodeId">目标节点 Id。</param>
        /// <returns>入边集合（新建列表）。</returns>
        public List<GraphEdge> GetEdgesToNode(string nodeId)
        {
            var result = new List<GraphEdge>();
            for (int i = 0; i < Edges.Count; i++)
            {
                if (Edges[i].TargetNodeId == nodeId)
                {
                    result.Add(Edges[i]);
                }
            }

            return result;
        }

        // -------- 编组 --------

        /// <summary>
        /// 按 Id 查找编组。
        /// </summary>
        /// <param name="groupId">编组 Id。</param>
        /// <returns>找到时返回编组，否则返回 <c>null</c>。</returns>
        public GraphGroup? GetGroup(string groupId)
        {
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].Id == groupId)
                {
                    return Groups[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 添加编组。Id 为空或已存在时失败。
        /// </summary>
        /// <param name="group">要添加的编组。</param>
        /// <returns>新增返回 <c>true</c>。</returns>
        public bool AddGroup(GraphGroup group)
        {
            if (group == null || string.IsNullOrEmpty(group.Id) || GetGroup(group.Id) != null)
            {
                return false;
            }

            Groups.Add(group);
            return true;
        }

        /// <summary>
        /// 移除编组。
        /// </summary>
        /// <param name="groupId">编组 Id。</param>
        /// <returns>移除成功返回 <c>true</c>。</returns>
        public bool RemoveGroup(string groupId)
        {
            for (int i = 0; i < Groups.Count; i++)
            {
                if (Groups[i].Id == groupId)
                {
                    Groups.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        // -------- 一致性校验 --------

        /// <summary>
        /// 校验图的一致性：边的源/目标节点与端口存在、编组引用的节点存在、节点/边/编组 Id 唯一。
        /// </summary>
        /// <param name="error">校验失败时的错误说明（成功时为 <c>null</c>）。</param>
        /// <returns>通过返回 <c>true</c>。</returns>
        public bool Validate(out string? error)
        {
            var nodeIds = new HashSet<string>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                var node = Nodes[i];
                if (string.IsNullOrEmpty(node.Id))
                {
                    error = $"Node at index {i} has an empty id.";
                    return false;
                }

                if (!nodeIds.Add(node.Id))
                {
                    error = $"Duplicate node id: {node.Id}";
                    return false;
                }
            }

            var edgeIds = new HashSet<string>();
            for (int i = 0; i < Edges.Count; i++)
            {
                var edge = Edges[i];
                if (string.IsNullOrEmpty(edge.Id))
                {
                    error = $"Edge at index {i} has an empty id.";
                    return false;
                }

                if (!edgeIds.Add(edge.Id))
                {
                    error = $"Duplicate edge id: {edge.Id}";
                    return false;
                }

                var source = GetNode(edge.SourceNodeId);
                if (source == null)
                {
                    error = $"Edge '{edge.Id}' references missing source node '{edge.SourceNodeId}'.";
                    return false;
                }

                var target = GetNode(edge.TargetNodeId);
                if (target == null)
                {
                    error = $"Edge '{edge.Id}' references missing target node '{edge.TargetNodeId}'.";
                    return false;
                }

                if (!source.HasPort(edge.SourcePort, GraphPortDirection.Output))
                {
                    error = $"Edge '{edge.Id}' references missing output port '{edge.SourcePort}' on node '{source.Id}'.";
                    return false;
                }

                if (!target.HasPort(edge.TargetPort, GraphPortDirection.Input))
                {
                    error = $"Edge '{edge.Id}' references missing input port '{edge.TargetPort}' on node '{target.Id}'.";
                    return false;
                }
            }

            var groupIds = new HashSet<string>();
            for (int i = 0; i < Groups.Count; i++)
            {
                var group = Groups[i];
                if (string.IsNullOrEmpty(group.Id))
                {
                    error = $"Group at index {i} has an empty id.";
                    return false;
                }

                if (!groupIds.Add(group.Id))
                {
                    error = $"Duplicate group id: {group.Id}";
                    return false;
                }

                for (int j = 0; j < group.NodeIds.Count; j++)
                {
                    if (!nodeIds.Contains(group.NodeIds[j]))
                    {
                        error = $"Group '{group.Id}' references missing node '{group.NodeIds[j]}'.";
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }

        // -------- 私有辅助 --------

        private int IndexOfNode(string nodeId)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i].Id == nodeId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void RemoveEdgesConnectingNode(string nodeId)
        {
            for (int i = Edges.Count - 1; i >= 0; i--)
            {
                if (Edges[i].ConnectsNode(nodeId))
                {
                    Edges.RemoveAt(i);
                }
            }
        }
    }
}
