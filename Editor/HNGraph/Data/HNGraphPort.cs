using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Graph.Editor
{
    [Serializable]
    public class HNGraphPort : IDisposable
    {
        public string Guid => guid;

        public string PortTypeName
        {
            get { return portTypeName; }
            set { portTypeName = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Direction PortDirection => portDirection;

        public Capacity PortCapacity => portCapacity;

        public string OwnerNodeGuid => ownerNodeGuid;

        public IReadOnlyList<string> EdgeGuids => edgeGuids;


        [SerializeField]
        protected string guid;

        [SerializeField]
        protected string portTypeName;

        [SerializeField]
        protected string name;

        [SerializeField]
        protected Direction portDirection;

        [SerializeField]
        protected Capacity portCapacity;

        [SerializeField]
        protected string ownerNodeGuid;

        [SerializeField]
        protected List<string> edgeGuids;
        

        public string PropertyName => propertyName;
        
        [SerializeField]
        private string propertyName;

        public HNGraphPort(string ownerNodeGuid, string typeName, string name, string propertyName, Direction direction, Capacity capacity)
        {
            guid = HNGraphUtils.NewGuid();
            
            this.ownerNodeGuid = ownerNodeGuid;
            this.portTypeName = typeName;
            this.name = name;
            this.portDirection = direction;
            this.portCapacity = capacity;
            
            edgeGuids = new List<string>();
            this.propertyName = propertyName;
        }

        public bool IsMatchWithAttribute(Type type, HNGraphPortInfo portInfo)
        {
            bool b = true;
            b &= this.portTypeName == type.FullName;
            b &= this.name == portInfo.PortName;
            b &= this.PortDirection == (portInfo.PortDirection == HNGraphPortInfo.Direction.Input ? Direction.Input : Direction.Output);
            b &= this.portCapacity == (portInfo.PortCapacity == HNGraphPortInfo.Capacity.Single ? Capacity.Single : Capacity.Multi);
            return b;
        }

        public void ConnectToEdge(HNGraphEdge edgeData)
        {
            if(edgeData == null)
                return;

            string edgeGuid = edgeData.Guid;
            if(edgeGuids.Contains(edgeGuid))
                return;

            edgeGuids.Add(edgeGuid);
        }

        public void DisconnectFromEdge(HNGraphEdge edgeData)
        {
            if(edgeData == null)
                return;

            string edgeGuid = edgeData.Guid;
            if(!edgeGuids.Contains(edgeGuid))
                return;
            
            edgeGuids.Remove(edgeGuid);
        }


        public List<HNGraphNode> GetConnectedNodes(bool isInputPort, HNGraphData editorData)
        {
            List<HNGraphNode> connectedNodes = new List<HNGraphNode>();
            if(editorData == null || edgeGuids.Count == 0)
                return connectedNodes;
            
            for(int i = 0; i < edgeGuids.Count; i++)
            {
                HNGraphEdge edge = editorData.GetEdge(edgeGuids[i]);
                HNGraphNode outputPortOwnerNode = editorData.GetNode(edge.GetOutputPort(editorData).OwnerNodeGuid);
                HNGraphNode inputPortOwnerNode = editorData.GetNode(edge.GetInputPort(editorData).OwnerNodeGuid);
                HNGraphNode node = isInputPort ? outputPortOwnerNode : inputPortOwnerNode;
                if(node == null)
                    continue;

                connectedNodes.Add(node);
            }

            return connectedNodes;
        }

        public void Dispose()
        {
            
        }


        [Serializable]
        public enum Direction
        {
            Input,
            Output
        }


        [Serializable]
        public enum Capacity
        {
            Single,
            Multi
        }
    }
}
