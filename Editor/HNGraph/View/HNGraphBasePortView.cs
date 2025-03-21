// using System;
// using System.Collections;
// using System.Collections.Generic;
// using JetBrains.Annotations;
// using UnityEditor.Experimental.GraphView;
// using UnityEngine;
// using UnityEngine.UIElements;

// namespace HN.Graph.Editor
// {
//     public abstract class HNGraphBasePortView : Port
//     {
//         public string PortName
//         {
//             get { return portName; }
//             set { portName = value; }
//         }
        
//         public HNGraphBasePort PortData => portData;

//         public HNGraphBaseNodeView OwnerNodeView => ownerNodeView;

//         public IReadOnlyList<HNGraphEdgeView> EdgeViews => edgeViews;

//         public HNGraphPortView RefPortView => refPortView;


//         protected HNGraphBasePort portData;
//         protected HNGraphPortView refPortView;
//         private HNGraphBaseNodeView ownerNodeView;

//         private List<HNGraphEdgeView> edgeViews;

//         private HNGraphView graphView;


//         public HNGraphBasePortView(
//             HNGraphView graphView,
//             HNGraphBasePort portData, 
//             HNGraphBaseNodeView nodeView, 
//             string name,
//             Orientation orientation,
//             Direction portDirection, 
//             Capacity capacity, 
//             IEdgeConnectorListener connectListener
//             ): base(orientation, portDirection, capacity, null)
//         {
//             this.graphView = graphView;
//             PortName = orientation == Orientation.Horizontal ? name : "";
//             tooltip = orientation == Orientation.Horizontal ? "" : name;
//             this.portData = portData;
//             this.ownerNodeView = nodeView;
//             edgeViews = new List<HNGraphEdgeView>();
            
//             m_EdgeConnector = new HNGraphEdgeConnector(graphView, connectListener);
//             this.AddManipulator(m_EdgeConnector);
//         }

//         public void ConnectToEdge(HNGraphEdgeView edgeView)
//         {
//             Connect(edgeView);
//             if(!edgeViews.Contains(edgeView))
//             {
//                 edgeViews.Add(edgeView);
//             }

//             portData.ConnectToEdge(edgeView.EdgeData);
//         }

//         public void DisconnectFromEdge(HNGraphEdgeView edgeView)
//         {
//             if(edgeViews.Contains(edgeView))
//             {
//                 Disconnect(edgeView);
//                 edgeViews.Remove(edgeView);
//                 OwnerNodeView.RefreshPorts();
//             }

//             portData.DisconnectFromEdge(edgeView.EdgeData);
//         }

//         public List<HNGraphBasePortView> GetConnectPorts()
//         {
//             List<HNGraphBasePortView> connectPorts = new List<HNGraphBasePortView>();
//             foreach(var edgeView in edgeViews)
//             {
//                  connectPorts.Add(edgeView.GetAnotherPort(this));
//             }

//             return connectPorts;
//         }

//         public HNGraphBasePortView GetFirstConnectPort()
//         {
//             if(edgeViews.Count > 0)
//             {
//                  return edgeViews[0].GetAnotherPort(this);
//             }
            
//             return null;
//         }

//         public List<HNGraphBaseNodeView> GetConnectNodes()
//         {
//             List<HNGraphBaseNodeView> connectNodes = new List<HNGraphBaseNodeView>();
//             List<HNGraphBasePortView> connectPorts = GetConnectPorts();
//             foreach(var port in connectPorts)
//             {
//                 connectNodes.Add(port.OwnerNodeView);
//             }

//             return connectNodes;
//         }

//         public HNGraphBaseNodeView GetFirstConnectNode()
//         {
//             HNGraphBasePortView connectPort = GetFirstConnectPort();
//             if(connectPort != null)
//             {
//                 return connectPort.OwnerNodeView;
//             }

//             return null;
//         }

//         public bool IsComptibleWith(HNGraphBasePortView portView)
//         {
//             return portView != null
//                 && portView.node != node
//                 && portView.direction != direction
//                 && (!portView.connected || (portView.connected && portView.capacity == Capacity.Multi));
//         }
//     }
// }
