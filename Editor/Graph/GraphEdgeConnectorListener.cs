using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 可自定义的边连接器监听器。默认情况下编辑器使用 GraphView 内置连接行为（经
    /// <c>graphViewChanged</c> 回流），领域编辑器可通过本类接管连线的落点处理
    /// （例如在空白处落下时自动创建节点）。
    /// </summary>
    public sealed class GraphEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly Action<GraphEditorView, Edge> _onDrop;
        private readonly Action<Edge> _onDropOutside;

        /// <summary>
        /// 初始化监听器。
        /// </summary>
        /// <param name="onDrop">在端口上完成连线时的回调。</param>
        /// <param name="onDropOutside">在空白处松开时的回调（可选）。</param>
        public GraphEdgeConnectorListener(Action<GraphEditorView, Edge> onDrop, Action<Edge> onDropOutside = null)
        {
            _onDrop = onDrop;
            _onDropOutside = onDropOutside;
        }

        /// <inheritdoc />
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            _onDropOutside?.Invoke(edge);
        }

        /// <inheritdoc />
        public void OnDrop(GraphView graphView, Edge edge)
        {
            if (!(graphView is GraphEditorView editorView))
            {
                return;
            }

            if (_onDrop != null)
            {
                _onDrop(editorView, edge);
                return;
            }

            editorView.AddDroppedEdge(edge);
        }
    }

    /// <summary>
    /// 端口边连接器工具。
    /// </summary>
    public static class GraphEdgeConnectors
    {
        /// <summary>
        /// 用自定义监听器替换端口上默认的边连接器。
        /// </summary>
        /// <param name="port">目标端口。</param>
        /// <param name="listener">自定义监听器。</param>
        public static void Attach(Port port, IEdgeConnectorListener listener)
        {
            if (port == null || listener == null)
            {
                return;
            }

            if (port.edgeConnector != null)
            {
                port.RemoveManipulator(port.edgeConnector);
            }

            port.AddManipulator(new EdgeConnector<Edge>(listener));
        }
    }
}
