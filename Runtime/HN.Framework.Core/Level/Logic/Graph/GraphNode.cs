#nullable enable

using MemoryPack;
using System.Collections.Generic;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 图节点。节点是图的基本单元，由唯一 Id 标识，
    /// 通过 <see cref="Type"/> 关联到领域图注册的节点类型，
    /// 持有端口集合（<see cref="Ports"/>）与键值属性包（<see cref="Properties"/>）。
    /// </summary>
    [MemoryPackable]
    public partial class GraphNode
    {
        /// <summary>节点唯一标识（图内唯一）。</summary>
        [MemoryPackOrder(0)]
        public string Id { get; set; }

        /// <summary>节点类型标识，由领域图 / 编辑器注册表解析。</summary>
        [MemoryPackOrder(1)]
        public string Type { get; set; }

        /// <summary>节点显示标题（可选，为空时由编辑器回退到 <see cref="Type"/>）。</summary>
        [MemoryPackOrder(2)]
        public string? Title { get; set; }

        /// <summary>节点在编辑器画布中的位置。</summary>
        [MemoryPackOrder(3)]
        public GraphVector2 Position { get; set; }

        /// <summary>节点端口集合。</summary>
        [MemoryPackOrder(4)]
        public List<GraphPort> Ports { get; set; }

        /// <summary>节点属性包（字符串键值，领域图可自行解释）。</summary>
        [MemoryPackOrder(5)]
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// 初始化空节点。
        /// </summary>
        public GraphNode()
        {
            Id = string.Empty;
            Type = string.Empty;
            Title = null;
            Ports = new List<GraphPort>();
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// 初始化节点。
        /// </summary>
        /// <param name="id">节点唯一标识。</param>
        /// <param name="type">节点类型标识。</param>
        /// <param name="position">节点位置。</param>
        public GraphNode(string id, string type, GraphVector2 position = default)
            : this()
        {
            Id = id;
            Type = type;
            Position = position;
        }

        /// <summary>
        /// 查找指定名称与方向的端口。
        /// </summary>
        /// <param name="name">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        /// <param name="port">找到的端口。</param>
        /// <returns>找到返回 <c>true</c>。</returns>
        public bool TryGetPort(string name, GraphPortDirection direction, out GraphPort port)
        {
            for (int i = 0; i < Ports.Count; i++)
            {
                var candidate = Ports[i];
                if (candidate.Direction == direction && candidate.Name == name)
                {
                    port = candidate;
                    return true;
                }
            }

            port = default;
            return false;
        }

        /// <summary>
        /// 是否存在指定名称与方向的端口。
        /// </summary>
        /// <param name="name">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        /// <returns>存在返回 <c>true</c>。</returns>
        public bool HasPort(string name, GraphPortDirection direction) =>
            TryGetPort(name, direction, out _);

        /// <summary>
        /// 添加端口。同名同方向的端口已存在时不重复添加。
        /// </summary>
        /// <param name="port">要添加的端口。</param>
        /// <returns>新增返回 <c>true</c>；已存在返回 <c>false</c>。</returns>
        public bool AddPort(GraphPort port)
        {
            if (HasPort(port.Name, port.Direction))
            {
                return false;
            }

            Ports.Add(port);
            return true;
        }

        /// <summary>
        /// 移除指定名称与方向的端口。
        /// </summary>
        /// <param name="name">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        /// <returns>移除成功返回 <c>true</c>；不存在返回 <c>false</c>。</returns>
        public bool RemovePort(string name, GraphPortDirection direction)
        {
            for (int i = 0; i < Ports.Count; i++)
            {
                var candidate = Ports[i];
                if (candidate.Direction == direction && candidate.Name == name)
                {
                    Ports.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 设置属性值。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <param name="value">属性值。</param>
        public void SetProperty(string key, string value) => Properties[key] = value;

        /// <summary>
        /// 读取属性值。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <param name="value">属性值。</param>
        /// <returns>存在返回 <c>true</c>。</returns>
        public bool TryGetProperty(string key, out string value) => Properties.TryGetValue(key, out value!);

        /// <summary>
        /// 移除属性。
        /// </summary>
        /// <param name="key">属性键。</param>
        /// <returns>移除成功返回 <c>true</c>。</returns>
        public bool RemoveProperty(string key) => Properties.Remove(key);
    }
}
