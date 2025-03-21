using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Graph.Editor
{
    [Serializable]
    public class SerializableNodes : SerializableDictionary<string, HNGraphNode> {}
    
    [Serializable]
    public class SerializableEdges : SerializableDictionary<string, HNGraphEdge> {}

    [Serializable]
    public class SerializablePorts : SerializableDictionary<string, HNGraphPort> {}

    [Serializable]
    public class SerializableGroups : SerializableDictionary<string, HNGraphGroup> {}

    [Serializable]
    public class SerializableStickyNotes : SerializableDictionary<string, HNGraphStickyNote> {}

    [Serializable]
    public class SerializableFloatingPanels : SerializableDictionary<string, IHNGraphFloatingPanel> {}
}
