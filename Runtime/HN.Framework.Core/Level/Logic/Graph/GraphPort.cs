#nullable enable

using MemoryPack;

namespace HN.Framework.Core.Level.Logic.Graph
{
    /// <summary>
    /// 图节点端口定义。端口是节点之间建立连接的锚点，
    /// 一条边（<see cref="GraphEdge"/>）从一个节点的输出端口连接到一个节点的输入端口。
    /// </summary>
    [MemoryPackable]
    public partial struct GraphPort
    {
        /// <summary>端口名称，在同一节点内唯一。</summary>
        [MemoryPackOrder(0)]
        public string Name { get; set; }

        /// <summary>端口方向（输入 / 输出）。</summary>
        [MemoryPackOrder(1)]
        public GraphPortDirection Direction { get; set; }

        /// <summary>端口数据类型标识（如 "Flow" / "float" / "GameplayTag"），由领域图自行约定。</summary>
        [MemoryPackOrder(2)]
        public string Type { get; set; }

        /// <summary>
        /// 初始化端口。
        /// </summary>
        /// <param name="name">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        /// <param name="type">端口数据类型标识。</param>
        public GraphPort(string name, GraphPortDirection direction, string type)
        {
            Name = name;
            Direction = direction;
            Type = type;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Name}({Direction}:{Type})";
    }
}
