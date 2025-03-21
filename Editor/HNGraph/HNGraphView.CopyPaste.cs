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
        private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements)
        {
            HNGraphCopyPasteData data = new HNGraphCopyPasteData();

            foreach(HNGraphNodeView nodeView in elements.Where(e => e is HNGraphNodeView))
            {
                HNGraphNode nodeData = nodeView.NodeData;
                if(!data.serializedNodes.Contains(nodeData))
                {
                    data.serializedNodes.Add(nodeData);
                }
            }

            foreach(HNGraphStickyNoteView stickyNoteView in elements.Where(e => e is HNGraphStickyNoteView))
            {
                HNGraphStickyNote stickyNoteData = stickyNoteView.StickyNoteData;
                if(!data.serializedStickyNotes.Contains(stickyNoteData))
                {
                    data.serializedStickyNotes.Add(stickyNoteData);
                }
            }

            return Json.Serialize(data);
        }

        private bool CanPasteSerializedDataCallback(string serializedData)
        {
            return true;
        }

        private void UnserializeAndPasteCallback(string operationName, string serializedData)
        {
            HNGraphCopyPasteData data = new HNGraphCopyPasteData();
            Json.DeserializeFromString(data, serializedData);
            
            GraphEditorData.Owner.RecordObject("Paste");

            foreach(var nodeData in data.serializedNodes)
            {
                AddNode(nodeData.NodeDataTypeName, nodeData.GetLayout().position + new Vector2(20, 20));
            }
            
            foreach(var nodeData in data.serializedNodes)
            {
                foreach(string inputPortGuid in nodeData.InputPortGuids)
                {
                    var inputPort = GraphEditorData.GetPort(inputPortGuid);
                    if(inputPort == null)
                    {
                        continue;
                    }

                    var edges = inputPort.EdgeGuids.ToList();
                    foreach(var edge in edges)
                    {
                        HNGraphPort connectPort = GraphEditorData.GetEdge(edge)?.GetOutputPort(GraphEditorData);
                        if(connectPort != null && connectPort.PortCapacity != HNGraphPort.Capacity.Single)
                        {
                            HNGraphEdge edgeData = new HNGraphEdge(connectPort, inputPort);
                            edgeData.Initialize();
                            GraphEditorData.AddEdge(edgeData);
                            HNGraphEdgeView edgeView = null;
                            AddEdgeView(edgeData, ref edgeView);
                        }
                    }
                }
            }

            foreach(var stickyNoteData in data.serializedStickyNotes)
            {
                AddStickyNote(stickyNoteData.GetLayout().position + new Vector2(20, 20));
            }

        }

    }
}
