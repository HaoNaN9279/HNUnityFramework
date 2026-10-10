using HN.Framework.Core.Level.Logic.Graph;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 图节点工厂：描述一种可创建的节点类型，并负责生成对应的 <see cref="GraphNode"/> 数据。
    /// 使用 <see cref="GraphNodeTypeAttribute"/> 标注后自动注册到 <see cref="GraphNodeRegistry"/>。
    /// </summary>
    public interface IGraphNodeFactory
    {
        /// <summary>节点类型唯一标识（写入 <see cref="GraphNode.Type"/>）。</summary>
        string TypeId { get; }

        /// <summary>节点在搜索菜单中的显示名称。</summary>
        string DisplayName { get; }

        /// <summary>
        /// 创建一个节点数据。
        /// </summary>
        /// <param name="id">节点唯一 Id。</param>
        /// <param name="position">节点在画布中的位置。</param>
        /// <returns>新建的节点数据。</returns>
        GraphNode Create(string id, GraphVector2 position);
    }
}
