using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace HN.Graph.Editor
{
    public class HNGraphNodeView : Node
    {        
        public HNGraphNode NodeData => nodeData;
        
        public HNGraphEdgeConnectorListener EdgeConnectorListener => edgeConnectorListener;


        public VisualElement TopPortContainer => topPortContainer;
        public VisualElement BottomPortContainer => bottomPortContainer;

        public IReadOnlyList<HNGraphPortView> InputPortViews => inputPortViews;
        public IReadOnlyList<HNGraphPortView> OutputPortViews => outputPortViews;

        public HNGraphView GraphView => graphView;


        protected HNGraphNode nodeData;

        protected HNGraphEdgeConnectorListener edgeConnectorListener;

        protected VisualElement topPortContainer;
        protected VisualElement bottomPortContainer;

        protected List<HNGraphPortView> inputPortViews;
        protected List<HNGraphPortView> outputPortViews;

        protected HNGraphView graphView;
        


        public HNGraphNodeView(HNGraphView graphView, HNGraphNode nodeData, HNGraphEdgeConnectorListener edgeConnectorListener)
        {
            this.graphView = graphView;
            this.edgeConnectorListener = edgeConnectorListener;
            this.nodeData = nodeData;

            topPortContainer = new VisualElement();
            topPortContainer.name = "TopPortContainer";
            this.Insert(0, topPortContainer);
            bottomPortContainer = new VisualElement();
            bottomPortContainer.name = "BottomPortContainer";
            this.Add(bottomPortContainer);

            inputPortViews = new List<HNGraphPortView>();
            outputPortViews = new List<HNGraphPortView>();
        }

        public void Initialize(HNGraphData editorData)
        {
            DrawNode(editorData);
            DrawPorts(editorData);
            SetPosition(nodeData.GetLayout());
        }

        protected void DrawNode(HNGraphData editorData)
        {
            Type nodeDataType = NodeData.GetNodeDataType(editorData);
            HNGraphNodeInfo info = nodeDataType.GetCustomAttribute<HNGraphNodeInfo>();
            title = info.NodeTitle;
            name = nodeDataType.Name;

            // string[] depths = info.MenuItem.Split('/');
            // foreach (string depth in depths)
            // {
            //     AddToClassList(depth.ToLower().Replace(' ', '-'));
            // }
        }

        protected void DrawPorts(HNGraphData editorData)
        {
            Type nodeDataType = NodeData.GetNodeDataType(editorData);
            FieldInfo[] fieldsInfo = nodeDataType.GetFields();
            foreach (var fieldInfo in fieldsInfo)
            {
                HNGraphPortInfo slotInfo = fieldInfo.GetCustomAttribute<HNGraphPortInfo>();
                if (slotInfo != null)
                {
                    HNGraphPort port = null;

                    foreach (string inputPortGuid in NodeData.InputPortGuids)
                    {
                        var inputPort = graphView.GraphEditorData.GetPort(inputPortGuid);
                        if (inputPort.IsMatchWithAttribute(fieldInfo.FieldType, slotInfo))
                        {
                            port = inputPort;
                        }
                    }

                    foreach (string outputPortGuid in NodeData.OutputPortGuids)
                    {
                        var outputPort = graphView.GraphEditorData.GetPort(outputPortGuid);
                        if (outputPort.IsMatchWithAttribute(fieldInfo.FieldType, slotInfo))
                        {
                            port = outputPort;
                        }
                    }

                    if (port == null)
                    {
                        port = new HNGraphPort(
                            NodeData.Guid,
                            fieldInfo.FieldType.FullName,
                            slotInfo.PortName,
                            fieldInfo.Name,
                            slotInfo.PortDirection == HNGraphPortInfo.Direction.Input ? HNGraphPort.Direction.Input : HNGraphPort.Direction.Output,
                            slotInfo.PortCapacity == HNGraphPortInfo.Capacity.Single ? HNGraphPort.Capacity.Single : HNGraphPort.Capacity.Multi
                            );

                        // if(port.PortDirection == HNGraphBasePort.Direction.Input)
                        //     BaseNodeData.AddInputPort(port);
                        // else if(port.PortDirection == HNGraphBasePort.Direction.Output)
                        //     BaseNodeData.AddOutputPort(port);
                    }

                    CreatePortView(editorData, port, slotInfo);
                }
            }
        }

        public void AddPortView(HNGraphData editorData, HNGraphPortView portView)
        {
            if(portView is not HNGraphPortView)
                return;

            if(portView.direction == Direction.Input)
            {
                if(portView.orientation == Orientation.Vertical)
                {
                    TopPortContainer.Add(portView);
                }
                else
                {
                    inputContainer.Add(portView);
                }
                inputPortViews.Add(portView as HNGraphPortView);

                nodeData.AddInputPort(editorData, portView.PortData);
            }
            else
            {
                if(portView.orientation == Orientation.Vertical)
                {
                    BottomPortContainer.Add(portView);
                }
                else
                {
                    outputContainer.Add(portView);
                }
                outputPortViews.Add(portView as HNGraphPortView);

                nodeData.AddOutputPort(editorData, portView.PortData);
            }
        }

        public void RemovePortView(HNGraphData editorData, HNGraphPortView portView)
        {
            if(inputContainer.Contains(portView))
            {
                inputContainer.Remove(portView);
                NodeData.RemoveInputPort(editorData, portView.PortData);
            }
            if(topPortContainer.Contains(portView))
            {
                topPortContainer.Remove(portView);
                NodeData.RemoveInputPort(editorData, portView.PortData);
            }
            if(outputContainer.Contains(portView))
            {
                outputContainer.Remove(portView);
                NodeData.RemoveOutputPort(editorData, portView.PortData);
            }
            if(bottomPortContainer.Contains(portView))
            {
                bottomPortContainer.Remove(portView);
                NodeData.RemoveOutputPort(editorData, portView.PortData);
            }
        }

        private void CreatePortView(HNGraphData editorData, HNGraphPort port, HNGraphPortInfo slotInfo)
        {
            HNGraphPortView portView = new HNGraphPortView(
                GraphView,
                port,
                this,
                slotInfo.PortName, 
                slotInfo.orientation == HNGraphPortInfo.Orientation.Horizontal ? Orientation.Horizontal : Orientation.Vertical,
                slotInfo.PortDirection == HNGraphPortInfo.Direction.Input ? Direction.Input : Direction.Output, 
                slotInfo.PortCapacity == HNGraphPortInfo.Capacity.Single ? Port.Capacity.Single : Port.Capacity.Multi,
                EdgeConnectorListener
                );
            AddPortView(editorData, portView);
        }

        public void SavePosition()
        {
            nodeData.SetLayout(GetPosition());
        }

    }
}
