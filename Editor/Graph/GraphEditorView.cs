using System;
using System.Collections.Generic;
using HN.Framework.Core.Level.Logic.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Framework.Editor.Graph
{
    /// <summary>
    /// 通用图编辑视图。基于 Unity GraphView，与 <see cref="GraphData"/> 数据模型双向同步。
    /// </summary>
    public class GraphEditorView : GraphView
    {
        private readonly Dictionary<string, GraphNodeView> _nodeViews = new Dictionary<string, GraphNodeView>();
        private readonly Dictionary<GraphNodeView, string> _nodeIds = new Dictionary<GraphNodeView, string>();
        private readonly Dictionary<Edge, GraphEdge> _edgeData = new Dictionary<Edge, GraphEdge>();
        private readonly Dictionary<Group, GraphGroup> _groupData = new Dictionary<Group, GraphGroup>();
        private readonly GraphNodeSearchProvider _searchProvider;
        private string _categoryFilter;

        /// <summary>
        /// 使用图数据初始化编辑视图。
        /// </summary>
        /// <param name="graph">图数据。</param>
        public GraphEditorView(GraphData graph)
        {
            style.flexGrow = 1f;

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var miniMap = new MiniMap { anchored = true };
            Add(miniMap);

            graphViewChanged = OnGraphViewChanged;
            nodeCreationRequest = OnNodeCreationRequest;

            _searchProvider = ScriptableObject.CreateInstance<GraphNodeSearchProvider>();
            _searchProvider.Initialize(this);

            LoadGraph(graph);
        }

        /// <summary>领域分类过滤（节点类型路径首段）。为空时展示全部节点类型。</summary>
        public string CategoryFilter
        {
            get => _categoryFilter;
            set
            {
                _categoryFilter = value;
                _searchProvider.CategoryFilter = value;
            }
        }

        /// <summary>当前图数据。</summary>
        public GraphData Graph { get; private set; }

        /// <summary>
        /// 从数据模型重新构建视图。会清空当前所有可视化元素。
        /// </summary>
        /// <param name="graph">图数据。</param>
        public void LoadGraph(GraphData graph)
        {
            ClearElements();
            Graph = graph ?? new GraphData();

            for (int i = 0; i < Graph.Nodes.Count; i++)
            {
                AddNodeView(Graph.Nodes[i]);
            }

            for (int i = 0; i < Graph.Edges.Count; i++)
            {
                AddEdgeView(Graph.Edges[i]);
            }

            for (int i = 0; i < Graph.Groups.Count; i++)
            {
                AddGroupView(Graph.Groups[i]);
            }
        }

        /// <summary>将所有可视化元素的位置同步回数据模型。</summary>
        public void SyncToData()
        {
            foreach (var pair in _nodeViews)
            {
                pair.Value.SyncPositionToData();
            }

            foreach (var pair in _groupData)
            {
                var position = pair.Key.GetPosition();
                pair.Value.Position = new GraphVector2(position.x, position.y);
                pair.Value.Size = new GraphVector2(position.width, position.height);
            }
        }

        /// <summary>
        /// 依据节点工厂创建节点数据并加入视图。
        /// </summary>
        /// <param name="factory">节点工厂。</param>
        /// <param name="screenPosition">鼠标屏幕坐标。</param>
        /// <returns>新建的节点数据。</returns>
        public GraphNode CreateNodeFromFactory(IGraphNodeFactory factory, Vector2 screenPosition)
        {
            if (factory == null)
            {
                return null;
            }

            var graphPosition = ScreenToGraphPosition(screenPosition);
            var node = factory.Create(NewId(), new GraphVector2(graphPosition.x, graphPosition.y));
            Graph.AddNode(node);
            AddNodeView(node);
            return node;
        }

        /// <summary>
        /// 处理自定义边连接器落下的边（领域编辑器接管连接时使用）。
        /// </summary>
        /// <param name="edge">落下的边。</param>
        public void AddDroppedEdge(Edge edge)
        {
            if (edge == null)
            {
                return;
            }

            AddElement(edge);
            RegisterEdge(edge);
        }

        /// <inheritdoc />
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();
            ports.ForEach(port =>
            {
                if (port == null || startPort == null || port == startPort)
                {
                    return;
                }

                if (port.node == startPort.node)
                {
                    return;
                }

                if (port.direction == startPort.direction)
                {
                    return;
                }

                compatible.Add(port);
            });

            return compatible;
        }

        private void OnNodeCreationRequest(NodeCreationContext context)
        {
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchProvider);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (change.elementsToRemove != null)
            {
                for (int i = 0; i < change.elementsToRemove.Count; i++)
                {
                    HandleElementRemoved(change.elementsToRemove[i]);
                }
            }

            if (change.edgesToCreate != null)
            {
                for (int i = 0; i < change.edgesToCreate.Count; i++)
                {
                    RegisterEdge(change.edgesToCreate[i]);
                }
            }

            if (change.movedElements != null)
            {
                for (int i = 0; i < change.movedElements.Count; i++)
                {
                    if (change.movedElements[i] is GraphNodeView nodeView)
                    {
                        nodeView.SyncPositionToData();
                        SyncNodeToGroup(nodeView);
                    }
                }
            }

            return change;
        }

        private void HandleElementRemoved(GraphElement element)
        {
            if (element is Edge edge && _edgeData.TryGetValue(edge, out var edgeData))
            {
                _edgeData.Remove(edge);
                Graph.RemoveEdge(edgeData.Id);
                return;
            }

            if (element is GraphNodeView nodeView && _nodeIds.TryGetValue(nodeView, out var nodeId))
            {
                _nodeIds.Remove(nodeView);
                _nodeViews.Remove(nodeId);
                Graph.RemoveNode(nodeId);
                return;
            }

            if (element is Group group && _groupData.TryGetValue(group, out var groupData))
            {
                _groupData.Remove(group);
                Graph.RemoveGroup(groupData.Id);
            }
        }

        private void RegisterEdge(Edge edge)
        {
            if (edge == null || _edgeData.ContainsKey(edge))
            {
                return;
            }

            if (!(edge.output?.userData is GraphPortReference outputRef) ||
                !(edge.input?.userData is GraphPortReference inputRef))
            {
                return;
            }

            var data = new GraphEdge(
                outputRef.NodeId,
                outputRef.PortName,
                inputRef.NodeId,
                inputRef.PortName,
                NewId());
            Graph.AddEdge(data);
            _edgeData[edge] = data;
        }

        private GraphNodeView AddNodeView(GraphNode data)
        {
            var view = new GraphNodeView(data);
            _nodeViews[data.Id] = view;
            _nodeIds[view] = data.Id;
            AddElement(view);
            return view;
        }

        private void AddEdgeView(GraphEdge data)
        {
            if (!_nodeViews.TryGetValue(data.SourceNodeId, out var source) ||
                !_nodeViews.TryGetValue(data.TargetNodeId, out var target))
            {
                return;
            }

            if (!source.TryGetPort(data.SourcePort, GraphPortDirection.Output, out var output) ||
                !target.TryGetPort(data.TargetPort, GraphPortDirection.Input, out var input))
            {
                return;
            }

            var edge = new Edge { output = output, input = input };
            output.Connect(edge);
            input.Connect(edge);
            AddElement(edge);
            _edgeData[edge] = data;
        }

        private void AddGroupView(GraphGroup data)
        {
            var group = new Group { title = data.Title };
            group.SetPosition(new Rect(data.Position.X, data.Position.Y, data.Size.X, data.Size.Y));
            AddElement(group);
            _groupData[group] = data;

            for (int i = 0; i < data.NodeIds.Count; i++)
            {
                if (_nodeViews.TryGetValue(data.NodeIds[i], out var nodeView))
                {
                    group.AddElement(nodeView);
                }
            }
        }

        private void SyncNodeToGroup(GraphNodeView nodeView)
        {
            foreach (var pair in _groupData)
            {
                bool contains = pair.Key.ContainsElement(nodeView);
                bool listed = pair.Value.NodeIds.Contains(nodeView.Data.Id);
                if (contains && !listed)
                {
                    pair.Value.AddNode(nodeView.Data.Id);
                }
                else if (!contains && listed)
                {
                    pair.Value.RemoveNode(nodeView.Data.Id);
                }
            }
        }

        private void ClearElements()
        {
            foreach (var view in new List<GraphNodeView>(_nodeViews.Values))
            {
                RemoveElement(view);
            }

            foreach (var edge in new List<Edge>(_edgeData.Keys))
            {
                RemoveElement(edge);
            }

            foreach (var group in new List<Group>(_groupData.Keys))
            {
                RemoveElement(group);
            }

            _nodeViews.Clear();
            _nodeIds.Clear();
            _edgeData.Clear();
            _groupData.Clear();
        }

        private Vector2 ScreenToGraphPosition(Vector2 screenPosition)
        {
            return contentViewContainer.WorldToLocal(screenPosition);
        }

        private static string NewId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
