using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using HN.Serialize;

namespace HN.Graph.Editor
{
    public partial class HNGraphView : GraphView
    {
        private static readonly string graphViewStyle = "Styles/HNGraphView";


        public HNGraphEditorWindow GraphEditorWindow;
        public HNGraphData GraphEditorData;
        public HNGraphSearchWindowProvider SearchWindowProvider;

        public IReadOnlyList<HNGraphNodeView> NodeViews => nodeViews;
        public IReadOnlyList<HNGraphEdgeView> EdgeViews => edgeViews;
        public IReadOnlyList<HNGraphGroupView> GroupViews => groupViews;
        public IReadOnlyList<HNGraphStickyNoteView> StickyNoteViews => stickyNoteViews;
        public IReadOnlyList<HNGraphFloatingPanelView> FloatingPanelViews => floatingPanelViews;

        public SelectionChanged OnSelectionChanged;
        public GraphViewElementsCreated graphViewElementsCreated;


        private List<HNGraphNodeView> nodeViews;
        private List<HNGraphEdgeView> edgeViews;
        private List<HNGraphGroupView> groupViews;
        private List<HNGraphStickyNoteView> stickyNoteViews;
        private List<HNGraphFloatingPanelView> floatingPanelViews;
        private HNGraphEdgeConnectorListener edgeConnectorListener;


        public HNGraphView(HNGraphEditorWindow graphEditorWindow, HNGraphData graphEditorData, HNGraphSearchWindowProvider searchWindowProvider)
        {
            this.GraphEditorWindow = graphEditorWindow;
            this.GraphEditorData = graphEditorData;
            this.SearchWindowProvider = searchWindowProvider;

            nodeViews = new List<HNGraphNodeView>();
            edgeViews = new List<HNGraphEdgeView>();
            groupViews = new List<HNGraphGroupView>();
            stickyNoteViews = new List<HNGraphStickyNoteView>();
            floatingPanelViews = new List<HNGraphFloatingPanelView>();

            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = UnserializeAndPasteCallback;


            edgeConnectorListener = new HNGraphEdgeConnectorListener(this);
            graphViewChanged = OnGraphViewChanged;

            styleSheets.Add(Resources.Load<StyleSheet>(graphViewStyle));
        }

        public void Initialize()
        {
            AddManipulator(new ContentDragger());
            AddManipulator(new SelectionDragger());
            AddManipulator(new RectangleSelector());

            GridBackground gridBackground = new GridBackground();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);

            SetupZoom(0.05f, 8);

            SearchWindowProvider.graph = this;
            nodeCreationRequest += context =>
            {
                Vector2 pos = context.screenMousePosition;
                var searchWindowContext = new SearchWindowContext(pos, 0f, 0f);
                SearchWindow.Open(searchWindowContext, SearchWindowProvider);
                EditorUtility.SetDirty(GraphEditorWindow);
            };

            DrawAllElements();
        }

        public void RedrawAllElements()
        {
            ClearAllElements();
            DrawAllElements();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            HNGraphPortView startPortView = startPort as HNGraphPortView;
            if(startPortView == null)
                return compatiblePorts;

            foreach (var nodeView in nodeViews)
            {
                List<HNGraphPortView> nodeViewPortViews = new List<HNGraphPortView>();
                if (startPortView.direction == Direction.Output)
                {
                    nodeViewPortViews = nodeView.InputPortViews.ToList();
                }
                else if (startPortView.direction == Direction.Input)
                {
                    nodeViewPortViews = nodeView.OutputPortViews.ToList();
                }
                foreach (var portView in nodeViewPortViews)
                {
                    if (startPortView.IsComptibleWith(portView))
                    {
                        compatiblePorts.Add(portView);
                    }
                }
            }

            return compatiblePorts;
        }

        public override void AddToSelection(ISelectable selectable)
        {
            base.AddToSelection(selectable);

            OnSelectionChanged?.Invoke(selection);
        }

        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);

            OnSelectionChanged?.Invoke(selection);
        }

        public override void ClearSelection()
        {
            base.ClearSelection();

            OnSelectionChanged?.Invoke(selection);
        }

        public void AddGraphElement(GraphElement graphElement)
        {
            if(graphElement is HNGraphNodeView)
                nodeViews.Add(graphElement as HNGraphNodeView);
            else if(graphElement is HNGraphEdgeView)
                edgeViews.Add(graphElement as HNGraphEdgeView);
            else if(graphElement is HNGraphGroupView)
                groupViews.Add(graphElement as HNGraphGroupView);
            else if(graphElement is HNGraphStickyNoteView)
                stickyNoteViews.Add(graphElement as HNGraphStickyNoteView);
            else if(graphElement is HNGraphFloatingPanelView)
                floatingPanelViews.Add(graphElement as HNGraphFloatingPanelView);
            else
                return;

            AddElement(graphElement);
        }

        public void RemoveGraphElement(GraphElement graphElement)
        {
            if(nodeViews.Contains(graphElement as HNGraphNodeView))
                nodeViews.Remove(graphElement as HNGraphNodeView);
            else if(edgeViews.Contains(graphElement as HNGraphEdgeView))
                edgeViews.Remove(graphElement as HNGraphEdgeView);
            else if(groupViews.Contains(graphElement as HNGraphGroupView))
                groupViews.Remove(graphElement as HNGraphGroupView);
            else if(stickyNoteViews.Contains(graphElement as HNGraphStickyNoteView))
                stickyNoteViews.Remove(graphElement as HNGraphStickyNoteView);
            else if(floatingPanelViews.Contains(graphElement as HNGraphFloatingPanelView))
                floatingPanelViews.Remove(graphElement as HNGraphFloatingPanelView);
            else
                return;

            RemoveElement(graphElement);
        }

        public bool ContainsElement(GraphElement graphElement)
        {
            return Contains(graphElement);
        }



        private void DrawAllElements()
        {
            DrawNodes();
            DrawEdges();
            DrawGroups();
            DrawStickyNotes();
        }

        private void ClearAllElements()
        {
            foreach(var stickyNote in stickyNoteViews)
            {
                RemoveElement(stickyNote);
            }
            stickyNoteViews.Clear();

            foreach(var group in groupViews)
            {
                RemoveElement(group);
            }
            groupViews.Clear();

            foreach(var edge in edgeViews)
            {
                RemoveElement(edge);
            }
            edgeViews.Clear();

            foreach(var node in nodeViews)
            {
                RemoveElement(node);
            }
            nodeViews.Clear();
        }


        private void AddManipulator(IManipulator manipulator)
        {
            if (manipulator != null)
            {
                manipulator.target = this;
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                List<HNGraphNodeView> nodeViews = graphViewChange.movedElements.OfType<HNGraphNodeView>().ToList();
                if(nodeViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Move Node");
                    for(int i = 0; i < nodeViews.Count; i++)
                    {
                        nodeViews[i].SavePosition();
                    }
                }

                List<HNGraphGroupView> groupViews = graphViewChange.movedElements.OfType<HNGraphGroupView>().ToList();
                if(groupViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Move Group");
                    for(int i = 0; i < groupViews.Count; i++)
                    {
                        groupViews[i].SavePosition();
                    }
                }

                List<HNGraphStickyNoteView> stickyNoteViews = graphViewChange.movedElements.OfType<HNGraphStickyNoteView>().ToList();
                if(stickyNoteViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Move Sticky Note");
                    for(int i = 0; i < stickyNoteViews.Count; i++)
                    {
                        stickyNoteViews[i].SavePosition();
                    }
                }
            }

            if (graphViewChange.elementsToRemove != null)
            {
                List<HNGraphNodeView> nodeViews = graphViewChange.elementsToRemove.OfType<HNGraphNodeView>().ToList();
                if (nodeViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Remove Node");
                    for (int i = nodeViews.Count - 1; i >= 0; i--)
                    {
                        RemoveNode(nodeViews[i]);
                    }
                }

                List<HNGraphEdgeView> edgeViews = graphViewChange.elementsToRemove.OfType<HNGraphEdgeView>().ToList();
                if (edgeViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Remove Edge");
                    for (int i = edgeViews.Count - 1; i >= 0; i--)
                    {
                        RemoveEdge(edgeViews[i]);
                    }
                }

                List<HNGraphGroupView> groupViews = graphViewChange.elementsToRemove.OfType<HNGraphGroupView>().ToList();
                if(groupViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Remove Group");
                    for(int i = groupViews.Count - 1; i >= 0; i--)
                    {
                        RemoveGroup(groupViews[i]);
                    }
                }

                List<HNGraphStickyNoteView> stickyNoteViews = graphViewChange.elementsToRemove.OfType<HNGraphStickyNoteView>().ToList();
                if(stickyNoteViews.Count > 0)
                {
                    GraphEditorData.Owner.RecordObject("Remove Sticky Note");
                    for(int i = stickyNoteViews.Count - 1; i >= 0; i--)
                    {
                        RemoveStickyNote(stickyNoteViews[i]);
                    }
                }
            }

            return graphViewChange;
        }

    
        public delegate void SelectionChanged(List<ISelectable> selection);

        public delegate HNGraphViewCreateElements GraphViewElementsCreated(HNGraphViewCreateElements graphViewChange);
    }

}
