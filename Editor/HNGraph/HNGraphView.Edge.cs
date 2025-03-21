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
        public void AddEdge(HNGraphEdgeView edgeView)
        {
            if (edgeView.output == null && edgeView.input == null)
                return;

            GraphEditorData.Owner.RecordObject("Add Edge");

            HNGraphEdge edgeData = edgeView.EdgeData;
            GraphEditorData.AddEdge(edgeData);

            // UpdateRefPortView(edgeView);
            
            AddEdgeView(edgeData, ref edgeView);

            graphViewElementsCreated(new HNGraphViewCreateElements(edgeView));
        }

        public void RemoveEdge(HNGraphEdgeView edgeView)
        {
            if(!ContainsElement(edgeView))
                return;    
            
            // UpdateDownstreamRelayNodeRefPort(edgeView, null);
            edgeView.DisconnectOutput();
            edgeView.DisconnectInput();
            GraphEditorData.RemoveEdge(edgeView.EdgeData);
            RemoveGraphElement(edgeView);
        }


        private void DrawEdges()
        {
            foreach(var edgeGuid in GraphEditorData.GetAllEdgeGuids())
            {
                HNGraphEdge edgeData = GraphEditorData.GetEdge(edgeGuid);
                HNGraphEdgeView edgeView = null;
                AddEdgeView(edgeData, ref edgeView);
            }

        }

        public void AddEdgeView(HNGraphEdge edgeData, ref HNGraphEdgeView edgeView)
        {
            if(edgeView == null)
            {
                edgeView = new HNGraphEdgeView(this);
                
                HNGraphNodeView outputPortNodeView = GetNodeViewFromGuid(edgeData.GetOutputPort(GraphEditorData).OwnerNodeGuid);
                string outputPortGuid = edgeData.GetOutputPort(GraphEditorData).Guid;
                HNGraphPortView outputPortView = null;
                foreach(var o in outputPortNodeView.OutputPortViews)
                    if(o.PortData.Guid == outputPortGuid)
                    {
                        outputPortView = o;
                        break;
                    }

                HNGraphNodeView inputPortNodeView = GetNodeViewFromGuid(edgeData.GetInputPort(GraphEditorData).OwnerNodeGuid);
                string inputPortGuid = edgeData.GetInputPort(GraphEditorData).Guid;
                HNGraphPortView inputPortView = null;
                foreach(var i in inputPortNodeView.InputPortViews)
                {
                    if(i.PortData.Guid == inputPortGuid)
                    {
                        inputPortView = i;
                        break;
                    }
                }

                edgeView.Initialize(edgeData, outputPortView, inputPortView);
            }
            AddGraphElement(edgeView);
        }

        // private void UpdateRefPortView(HNGraphEdgeView edgeView)
        // {
        //     HNGraphPortView refPortView = null;
        //     refPortView = FindUpstreamRefPortView(edgeView);
        //     if(refPortView == null)
        //     {
        //         refPortView = FindDownstreamRefPortView(edgeView);
        //     }

        //     if(refPortView == null)
        //         return;

        //     UpdateUpstreamRelayNodeRefPort(edgeView, refPortView);
        //     UpdateDownstreamRelayNodeRefPort(edgeView, refPortView);
        // }

        // private HNGraphPortView FindUpstreamRefPortView(HNGraphEdgeView edgeView)
        // {
        //     HNGraphBasePortView edgeOutputPortView = edgeView.OutputPortView;
        //     HNGraphBaseNodeView edgeOutputPortOwnerNodeView = edgeOutputPortView.OwnerNodeView;
        //     if(edgeOutputPortOwnerNodeView is HNGraphRelayNodeView)
        //     {
        //         HNGraphRelayNodeView upstreamRelayNodeView = edgeOutputPortOwnerNodeView as HNGraphRelayNodeView;
        //         var upstreamRelayNodeViewInputEdgeView = upstreamRelayNodeView.InputPortView.EdgeViews;
        //         if(upstreamRelayNodeViewInputEdgeView.Count > 0)
        //         {
        //             HNGraphEdgeView nextEdgeView = upstreamRelayNodeViewInputEdgeView[0];
        //             return FindUpstreamRefPortView(nextEdgeView);
        //         }
        //     }
        //     else if(edgeOutputPortOwnerNodeView is HNGraphNodeView)
        //     {
        //         return edgeOutputPortView as HNGraphPortView;
        //     }

        //     return null;
        // }

        // private HNGraphPortView FindDownstreamRefPortView(HNGraphEdgeView edgeView)
        // {
        //     HNGraphBasePortView edgeInputPortView = edgeView.InputPortView;
        //     HNGraphBaseNodeView edgeInputPortOwnerNodeView = edgeInputPortView.OwnerNodeView;
        //     if(edgeInputPortOwnerNodeView is HNGraphRelayNodeView)
        //     {
        //         HNGraphRelayNodeView upstreamRelayNodeView = edgeInputPortOwnerNodeView as HNGraphRelayNodeView;
        //         var upstreamRelayNodeViewOutputEdgeView = upstreamRelayNodeView.OutputPortView.EdgeViews;
        //         if(upstreamRelayNodeViewOutputEdgeView.Count > 0)
        //         {
        //             HNGraphEdgeView nextEdgeView = upstreamRelayNodeViewOutputEdgeView[0];
        //             return FindDownstreamRefPortView(nextEdgeView);
        //         }
        //     }
        //     else if(edgeInputPortOwnerNodeView is HNGraphNodeView)
        //     {
        //         return edgeInputPortView as HNGraphPortView;
        //     }

        //     return null;
        // }

        // private void UpdateUpstreamRelayNodeRefPort(HNGraphEdgeView edgeView, HNGraphPortView newRefPortView)
        // {
        //     HNGraphBasePortView edgeOutputPortView = edgeView.OutputPortView;
        //     HNGraphBaseNodeView edgeOutputPortOwnerNodeView = edgeOutputPortView.OwnerNodeView;
        //     if(edgeOutputPortOwnerNodeView is HNGraphRelayNodeView)
        //     {
        //         HNGraphRelayNodeView upstreamRelayNodeView = edgeOutputPortOwnerNodeView as HNGraphRelayNodeView;
        //         if(newRefPortView == null)
        //         {
        //             upstreamRelayNodeView.RelayNodeData.SetRefPort(GraphEditorData, null);
        //         }
        //         else
        //         {
        //             upstreamRelayNodeView.RelayNodeData.SetRefPort(GraphEditorData, newRefPortView.PortData);
        //         }
        //         var upstreamRelayNodeViewInputEdgeView = upstreamRelayNodeView.InputPortView.EdgeViews;
        //         foreach(var inputEdgeView in upstreamRelayNodeViewInputEdgeView)
        //         {
        //             UpdateUpstreamRelayNodeRefPort(inputEdgeView, newRefPortView);
        //         }
        //     }

        //     return;
        // }

        // private void UpdateDownstreamRelayNodeRefPort(HNGraphEdgeView edgeView, HNGraphPortView newRefPortView)
        // {
        //     HNGraphBasePortView edgeInputPortView = edgeView.InputPortView;
        //     HNGraphBaseNodeView edgeInputPortOwnerNodeView = edgeInputPortView.OwnerNodeView;
        //     if(edgeInputPortOwnerNodeView is HNGraphRelayNodeView)
        //     {
        //         HNGraphRelayNodeView upstreamRelayNodeView = edgeInputPortOwnerNodeView as HNGraphRelayNodeView;
        //         if(newRefPortView == null)
        //         {
        //             upstreamRelayNodeView.RelayNodeData.SetRefPort(GraphEditorData, null);
        //         }
        //         else
        //         {
        //             upstreamRelayNodeView.RelayNodeData.SetRefPort(GraphEditorData, newRefPortView.PortData);
        //         }
        //         var upstreamRelayNodeViewOutputEdgeView = upstreamRelayNodeView.OutputPortView.EdgeViews;
        //         foreach(var outputEdgeView in upstreamRelayNodeViewOutputEdgeView)
        //         {
        //             UpdateDownstreamRelayNodeRefPort(outputEdgeView, newRefPortView);
        //         }
        //     }

        //     return;
        // }
        

        private HNGraphEdgeView GetEdgeViewFromGuid(string guid)
        {
            foreach(var edgeView in edgeViews)
            {
                if(guid == edgeView.EdgeData.Guid)
                    return edgeView;
            }
            return null;
        }

    }
}
