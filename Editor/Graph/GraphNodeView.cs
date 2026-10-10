using System.Collections.Generic;
using HN.Framework.Core.Level.Logic.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 图节点的可视化元素。绑定一个 <see cref="GraphNode"/> 数据，并镜像其端口集合。
    /// </summary>
    public sealed class GraphNodeView : Node
    {
        private readonly Dictionary<string, Port> _inputPorts = new Dictionary<string, Port>();
        private readonly Dictionary<string, Port> _outputPorts = new Dictionary<string, Port>();

        /// <summary>
        /// 使用节点数据初始化可视化节点。
        /// </summary>
        /// <param name="data">节点数据。</param>
        public GraphNodeView(GraphNode data)
        {
            Data = data;
            title = string.IsNullOrEmpty(data.Title) ? data.Type : data.Title;
            SetPosition(new Rect(data.Position.X, data.Position.Y, 0f, 0f));
            BuildPorts();
            RefreshExpandedState();
            RefreshPorts();
        }

        /// <summary>绑定的节点数据。</summary>
        public GraphNode Data { get; }

        /// <summary>
        /// 按名称与方向获取端口。
        /// </summary>
        /// <param name="name">端口名称。</param>
        /// <param name="direction">端口方向。</param>
        /// <param name="port">找到的端口。</param>
        /// <returns>找到返回 <c>true</c>。</returns>
        public bool TryGetPort(string name, GraphPortDirection direction, out Port port)
        {
            var map = direction == GraphPortDirection.Input ? _inputPorts : _outputPorts;
            return map.TryGetValue(name, out port);
        }

        /// <summary>将可视化位置同步回节点数据。</summary>
        public void SyncPositionToData()
        {
            var position = GetPosition();
            Data.Position = new GraphVector2(position.x, position.y);
        }

        /// <summary>将可视化标题同步回节点数据。</summary>
        public void SyncTitleToData()
        {
            Data.Title = title;
        }

        private void BuildPorts()
        {
            for (int i = 0; i < Data.Ports.Count; i++)
            {
                var definition = Data.Ports[i];
                bool isInput = definition.Direction == GraphPortDirection.Input;
                var port = InstantiatePort(
                    Orientation.Horizontal,
                    isInput ? Direction.Input : Direction.Output,
                    Port.Capacity.Multi,
                    typeof(object));
                port.portName = definition.Name;
                port.userData = new GraphPortReference(Data.Id, definition.Name, definition.Direction);

                if (isInput)
                {
                    _inputPorts[definition.Name] = port;
                    inputContainer.Add(port);
                }
                else
                {
                    _outputPorts[definition.Name] = port;
                    outputContainer.Add(port);
                }
            }
        }
    }
}
