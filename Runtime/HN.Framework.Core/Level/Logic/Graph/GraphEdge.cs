#nullable enable

using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 图连接边。表示从一个节点的输出端口指向另一个节点的输入端口的连接。
    /// </summary>
    [MemoryPackable]
    public partial class GraphEdge
    {
        /// <summary>边唯一标识（图内唯一）。</summary>
        [MemoryPackOrder(0)]
        public string Id { get; set; }

        /// <summary>源节点 Id。</summary>
        [MemoryPackOrder(1)]
        public string SourceNodeId { get; set; }

        /// <summary>源端口名称。</summary>
        [MemoryPackOrder(2)]
        public string SourcePort { get; set; }

        /// <summary>目标节点 Id。</summary>
        [MemoryPackOrder(3)]
        public string TargetNodeId { get; set; }

        /// <summary>目标端口名称。</summary>
        [MemoryPackOrder(4)]
        public string TargetPort { get; set; }

        /// <summary>
        /// 初始化空边。
        /// </summary>
        public GraphEdge()
        {
            Id = string.Empty;
            SourceNodeId = string.Empty;
            SourcePort = string.Empty;
            TargetNodeId = string.Empty;
            TargetPort = string.Empty;
        }

        /// <summary>
        /// 初始化边。
        /// </summary>
        /// <param name="sourceNodeId">源节点 Id。</param>
        /// <param name="sourcePort">源端口名称。</param>
        /// <param name="targetNodeId">目标节点 Id。</param>
        /// <param name="targetPort">目标端口名称。</param>
        /// <param name="id">边 Id，为空时使用空的 Id。</param>
        public GraphEdge(string sourceNodeId, string sourcePort, string targetNodeId, string targetPort, string? id = null)
        {
            Id = id ?? string.Empty;
            SourceNodeId = sourceNodeId;
            SourcePort = sourcePort;
            TargetNodeId = targetNodeId;
            TargetPort = targetPort;
        }

        /// <summary>
        /// 判断该边是否连接了指定节点（作为源或目标）。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>连接返回 <c>true</c>。</returns>
        public bool ConnectsNode(string nodeId) =>
            SourceNodeId == nodeId || TargetNodeId == nodeId;

        /// <inheritdoc />
        public override string ToString() =>
            $"{SourceNodeId}.{SourcePort} -> {TargetNodeId}.{TargetPort}";
    }
}
