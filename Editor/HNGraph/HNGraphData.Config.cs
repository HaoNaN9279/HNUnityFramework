using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HN.Serialize;

namespace HN.Graph.Editor
{
    public abstract partial class HNGraphData
    {
        [SerializeField]
        internal string GraphEditorAssemblyName = "HN.Graph.Editor";

        [SerializeField]
        internal string GraphRuntimeAssemblyName = "HN.Graph";

        [SerializeField]
        internal string GraphNodeDataNamespace = "HN";
    }
}
