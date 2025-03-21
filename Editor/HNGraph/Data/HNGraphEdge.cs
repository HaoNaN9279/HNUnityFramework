using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace HN.Graph.Editor
{
    [Serializable]
    public class HNGraphEdge : IDisposable
    {
        public string Guid => guid;

        public string OutputPortGuid => outputPortGuid;
        public string InputPortGuid => inputPortGuid;


        [SerializeField]
        private string guid;

        [SerializeField]
        private string outputPortGuid;

        [SerializeField]
        private string inputPortGuid;
        

        public HNGraphEdge(HNGraphPort outputPort, HNGraphPort inputPort)
        {
            this.outputPortGuid = outputPort.Guid;
            this.inputPortGuid = inputPort.Guid;
        }

        public virtual void Initialize()
        {
            guid = HNGraphUtils.NewGuid();
            // editorData.GetBasePort(outputPortGuid)?.ConnectToEdge(this);
            // editorData.GetBasePort(inputPortGuid)?.ConnectToEdge(this);
        }

        public virtual void Dispose()
        {
            outputPortGuid = null;
            inputPortGuid = null;
        }

        public HNGraphPort GetOutputPort(HNGraphData editorData)
        {
            return editorData.GetPort(outputPortGuid);
        }

        public HNGraphPort GetInputPort(HNGraphData editorData)
        {
            return editorData.GetPort(inputPortGuid);
        }

    }
}
