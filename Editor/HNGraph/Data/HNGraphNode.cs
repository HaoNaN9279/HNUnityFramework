using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HN.Serialize;
using UnityEngine;

namespace HN.Graph.Editor
{
    [Serializable]
    public class HNGraphNode : IDisposable, IPositionable
    {
        public string Guid => guid;
        public JsonData NodeData => nodeData;

        public string NodeDataTypeName => nodeDataTypeName;

        public IReadOnlyList<string> InputPortGuids => inputPortGuids;

        public IReadOnlyList<string> OutputPortGuids => outputPortGuids;


        [SerializeField]
        protected string guid;

        [SerializeField]
        protected Rect layout;

        [SerializeField]
        protected List<string> inputPortGuids;
        
        [SerializeField]
        protected List<string> outputPortGuids;

        [SerializeField]
        private JsonData nodeData;

        [SerializeField]
        private string nodeDataTypeName;


        public HNGraphNode(string nodeDataTypeName)
        {
            this.nodeDataTypeName = nodeDataTypeName;

            inputPortGuids = new List<string>();
            outputPortGuids = new List<string>();
        }

        public void Initialize(Vector2 position, HNGraphData editorData)
        {
            guid = HNGraphUtils.NewGuid();
            SetLayout(new Rect(position, Vector2.zero));

            Assembly assembly = Assembly.Load(editorData.GraphRuntimeAssemblyName);
            if(assembly != null)
            {
                Type nodeDataType = assembly.GetType($"{editorData.GraphNodeDataNamespace}.{nodeDataTypeName}");
                if(nodeDataType == null)
                    return;

                JsonObject jsonObject = Activator.CreateInstance(nodeDataType) as JsonObject;
                if(jsonObject == null)
                    return;
                
                nodeData = new JsonData(jsonObject);
            }
        }

        public Type GetNodeDataType(HNGraphData editorData)
        {
            Assembly assembly = Assembly.Load(editorData.GraphRuntimeAssemblyName);
            if(assembly != null)
            {
                return assembly.GetType($"{editorData.GraphNodeDataNamespace}.{nodeDataTypeName}");
            }

            return null;
        }

        public void AddInputPort(HNGraphData editorData, HNGraphPort port)
        {
            if(port == null || port is not HNGraphPort)
                return;

            if(inputPortGuids.Contains(port.Guid))
                return;

            editorData.AddPort(port as HNGraphPort);
            inputPortGuids.Add(port.Guid);
        }

        public void AddOutputPort(HNGraphData editorData, HNGraphPort port)
        {
            if(port == null || port is not HNGraphPort)
                return;

            if(outputPortGuids.Contains(port.Guid))
                return;

            editorData.AddPort(port as HNGraphPort);
            outputPortGuids.Add(port.Guid);
        }

        public void RemoveInputPort(HNGraphData editorData, HNGraphPort port)
        {
            editorData.RemovePort(port as HNGraphPort);
            inputPortGuids.Remove(port.Guid);
        }

        public void RemoveOutputPort(HNGraphData editorData, HNGraphPort port)
        {
            editorData.RemovePort(port as HNGraphPort);
            outputPortGuids.Remove(port.Guid);
        }

        public HNGraphPort GetInputPort(HNGraphData editorData, string guid)
        {
            return editorData.GetPort(guid);
        }

        public HNGraphPort GetOutputPort(HNGraphData editorData, string guid)
        {
            return editorData.GetPort(guid);
        }

        public Rect GetLayout()
        {
            return this.layout;
        }

        public void SetLayout(Rect layout)
        {
            this.layout = layout;
        }

        public void Dispose()
        {
        }

    }
}
