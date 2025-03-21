using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace HN.Graph.Editor
{
    public class HNGraphPortView : Port
    {
        public string PortName
        {
            get { return portName; }
            set { portName = value; }
        }
        
        public HNGraphPort PortData => portData;

        public HNGraphNodeView OwnerNodeView => ownerNodeView;

        public IReadOnlyList<HNGraphEdgeView> EdgeViews => edgeViews;

        public HNGraphPortView RefPortView => refPortView;


        protected HNGraphPort portData;
        protected HNGraphPortView refPortView;
        private HNGraphNodeView ownerNodeView;

        private List<HNGraphEdgeView> edgeViews;

        private HNGraphView graphView;


        public HNGraphPortView(
            HNGraphView graphView, 
            HNGraphPort portData, 
            HNGraphNodeView nodeView, 
            string name, 
            Orientation orientation, 
            Direction portDirection, 
            Capacity capacity, 
            IEdgeConnectorListener connectListener
            ): base(orientation, portDirection, capacity, null)
        {
            this.graphView = graphView;
            PortName = orientation == Orientation.Horizontal ? name : "";
            tooltip = orientation == Orientation.Horizontal ? "" : name;
            this.portData = portData;
            this.ownerNodeView = nodeView;
            edgeViews = new List<HNGraphEdgeView>();
            
            m_EdgeConnector = new HNGraphEdgeConnector(graphView, connectListener);
            this.AddManipulator(m_EdgeConnector);
        }

        public void ConnectToEdge(HNGraphEdgeView edgeView)
        {
            Connect(edgeView);
            if(!edgeViews.Contains(edgeView))
            {
                edgeViews.Add(edgeView);
            }

            portData.ConnectToEdge(edgeView.EdgeData);
        }

        public void DisconnectFromEdge(HNGraphEdgeView edgeView)
        {
            if(edgeViews.Contains(edgeView))
            {
                Disconnect(edgeView);
                edgeViews.Remove(edgeView);
                OwnerNodeView.RefreshPorts();
            }

            portData.DisconnectFromEdge(edgeView.EdgeData);
        }

        public List<HNGraphPortView> GetConnectPorts()
        {
            List<HNGraphPortView> connectPorts = new List<HNGraphPortView>();
            foreach(var edgeView in edgeViews)
            {
                 connectPorts.Add(edgeView.GetAnotherPort(this));
            }

            return connectPorts;
        }

        public HNGraphPortView GetFirstConnectPort()
        {
            if(edgeViews.Count > 0)
            {
                 return edgeViews[0].GetAnotherPort(this);
            }
            
            return null;
        }

        public List<HNGraphNodeView> GetConnectNodes()
        {
            List<HNGraphNodeView> connectNodes = new List<HNGraphNodeView>();
            List<HNGraphPortView> connectPorts = GetConnectPorts();
            foreach(var port in connectPorts)
            {
                connectNodes.Add(port.OwnerNodeView);
            }

            return connectNodes;
        }

        public HNGraphNodeView GetFirstConnectNode()
        {
            HNGraphPortView connectPort = GetFirstConnectPort();
            if(connectPort != null)
            {
                return connectPort.OwnerNodeView;
            }

            return null;
        }

        public bool IsComptibleWith(HNGraphPortView portView)
        {
            return portView != null
                && portView.node != node
                && portView.direction != direction
                && (!portView.connected || (portView.connected && portView.capacity == Capacity.Multi));
        }
    }
}
