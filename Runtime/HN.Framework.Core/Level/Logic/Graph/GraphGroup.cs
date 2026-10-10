#nullable enable

using MemoryPack;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 节点编组。用于在编辑器中将若干节点作为一个可拖拽的整体进行视觉分组，
    /// 不参与运行时逻辑求值。
    /// </summary>
    [MemoryPackable]
    public partial class GraphGroup
    {
        /// <summary>编组唯一标识（图内唯一）。</summary>
        [MemoryPackOrder(0)]
        public string Id { get; set; }

        /// <summary>编组显示标题。</summary>
        [MemoryPackOrder(1)]
        public string Title { get; set; }

        /// <summary>编组包含的节点 Id 集合。</summary>
        [MemoryPackOrder(2)]
        public List<string> NodeIds { get; set; }

        /// <summary>编组在编辑器画布中的位置。</summary>
        [MemoryPackOrder(3)]
        public GraphVector2 Position { get; set; }

        /// <summary>编组在编辑器画布中的尺寸。</summary>
        [MemoryPackOrder(4)]
        public GraphVector2 Size { get; set; }

        /// <summary>
        /// 初始化空编组。
        /// </summary>
        public GraphGroup()
        {
            Id = string.Empty;
            Title = string.Empty;
            NodeIds = new List<string>();
            Position = GraphVector2.Zero;
            Size = GraphVector2.Zero;
        }

        /// <summary>
        /// 初始化编组。
        /// </summary>
        /// <param name="id">编组唯一标识。</param>
        /// <param name="title">编组标题。</param>
        public GraphGroup(string id, string title)
            : this()
        {
            Id = id;
            Title = title;
        }

        /// <summary>
        /// 将节点加入编组。同一节点不重复加入。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>新增返回 <c>true</c>；已在编组中返回 <c>false</c>。</returns>
        public bool AddNode(string nodeId)
        {
            if (NodeIds.Contains(nodeId))
            {
                return false;
            }

            NodeIds.Add(nodeId);
            return true;
        }

        /// <summary>
        /// 从编组移除节点。
        /// </summary>
        /// <param name="nodeId">节点 Id。</param>
        /// <returns>移除成功返回 <c>true</c>。</returns>
        public bool RemoveNode(string nodeId) => NodeIds.Remove(nodeId);
    }
}
