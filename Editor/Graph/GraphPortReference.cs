using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 附加到 GraphView 端口上的元数据，用于在可视化元素与 <see cref="GraphNode"/> 数据之间建立映射。
    /// 通过 <c>Port.userData</c> 关联。
    /// </summary>
    public sealed class GraphPortReference
    {
        /// <summary>
        /// 初始化端口引用。
        /// </summary>
        /// <param name="nodeId">所属节点 Id。</param>
        /// <param name="portName">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        public GraphPortReference(string nodeId, string portName, GraphPortDirection direction)
        {
            NodeId = nodeId;
            PortName = portName;
            Direction = direction;
        }

        /// <summary>所属节点 Id。</summary>
        public string NodeId { get; }

        /// <summary>端口名称。</summary>
        public string PortName { get; }

        /// <summary>端口方向。</summary>
        public GraphPortDirection Direction { get; }
    }
}
