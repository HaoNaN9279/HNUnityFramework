// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// namespace HN.Graph.Editor
// {
//     [Serializable]
//     public abstract class HNGraphBasePort : IDisposable
//     {
//         public string Guid => guid;

//         public string PortTypeName
//         {
//             get { return portTypeName; }
//             set { portTypeName = value; }
//         }

//         public string Name
//         {
//             get { return name; }
//             set { name = value; }
//         }

//         public Direction PortDirection => portDirection;

//         public Capacity PortCapacity => portCapacity;

//         public string OwnerNodeGuid => ownerNodeGuid;

//         public IReadOnlyList<string> EdgeGuids => edgeGuids;

//         // public HNGraphData EditorData
//         // {
//         //     set { editorData = value; }
//         // }


//         [SerializeField]
//         protected string guid;

//         [SerializeField]
//         protected string portTypeName;

//         [SerializeField]
//         protected string name;

//         [SerializeField]
//         protected Direction portDirection;

//         [SerializeField]
//         protected Capacity portCapacity;

//         [SerializeField]
//         protected string ownerNodeGuid;

//         [SerializeField]
//         protected List<string> edgeGuids;
        
//         // protected HNGraphData editorData;


//         public HNGraphBasePort(string ownerNodeGuid, string typeName, string name, Direction direction, Capacity capacity)
//         {
//             guid = HNGraphUtils.NewGuid();
            
//             this.ownerNodeGuid = ownerNodeGuid;
//             this.portTypeName = typeName;
//             this.name = name;
//             this.portDirection = direction;
//             this.portCapacity = capacity;
            
//             edgeGuids = new List<string>();
//         }

//         public bool IsMatchWithAttribute(Type type, HNGraphPortInfo portInfo)
//         {
//             bool b = true;
//             b &= this.portTypeName == type.FullName;
//             b &= this.name == portInfo.PortName;
//             b &= this.PortDirection == (portInfo.PortDirection == HNGraphPortInfo.Direction.Input ? Direction.Input : Direction.Output);
//             b &= this.portCapacity == (portInfo.PortCapacity == HNGraphPortInfo.Capacity.Single ? Capacity.Single : Capacity.Multi);
//             return b;
//         }

//         public virtual void ConnectToEdge(HNGraphEdge edgeData)
//         {
//             if(edgeData == null)
//                 return;

//             string edgeGuid = edgeData.Guid;
//             if(edgeGuids.Contains(edgeGuid))
//                 return;

//             edgeGuids.Add(edgeGuid);
//         }

//         public virtual void DisconnectFromEdge(HNGraphEdge edgeData)
//         {
//             if(edgeData == null)
//                 return;

//             string edgeGuid = edgeData.Guid;
//             if(!edgeGuids.Contains(edgeGuid))
//                 return;
            
//             edgeGuids.Remove(edgeGuid);
//         }

//         public abstract List<HNGraphNode> GetConnectedNodes(bool isInputPort, HNGraphData editorData);

//         public virtual void Dispose()
//         {
            
//         }


//         [Serializable]
//         public enum Direction
//         {
//             Input,
//             Output
//         }


//         [Serializable]
//         public enum Capacity
//         {
//             Single,
//             Multi
//         }
//     }
// }
