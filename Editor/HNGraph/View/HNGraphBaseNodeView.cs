// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor.Experimental.GraphView;
// using UnityEditor.UIElements;
// using UnityEngine;
// using UnityEngine.UIElements;

// namespace HN.Graph.Editor
// {
//     public abstract class HNGraphBaseNodeView : Node
//     {
//         public HNGraphBaseNode BaseNodeData => baseNodeData;
        
//         public HNGraphEdgeConnectorListener EdgeConnectorListener => edgeConnectorListener;


//         public VisualElement TopPortContainer => topPortContainer;
//         public VisualElement BottomPortContainer => bottomPortContainer;

//         public IReadOnlyList<HNGraphBasePortView> InputPortViews => inputPortViews;
//         public IReadOnlyList<HNGraphBasePortView> OutputPortViews => outputPortViews;

//         public HNGraphView GraphView => graphView;


//         protected HNGraphBaseNode baseNodeData;

//         protected HNGraphEdgeConnectorListener edgeConnectorListener;

//         protected VisualElement topPortContainer;
//         protected VisualElement bottomPortContainer;

//         protected List<HNGraphBasePortView> inputPortViews;
//         protected List<HNGraphBasePortView> outputPortViews;

//         protected HNGraphView graphView;


//         public HNGraphBaseNodeView(HNGraphView graphView, HNGraphBaseNode nodeData, HNGraphEdgeConnectorListener edgeConnectorListener)
//         {
//             this.graphView = graphView;
//             this.edgeConnectorListener = edgeConnectorListener;
//             this.baseNodeData = nodeData;

//             topPortContainer = new VisualElement();
//             topPortContainer.name = "TopPortContainer";
//             this.Insert(0, topPortContainer);
//             bottomPortContainer = new VisualElement();
//             bottomPortContainer.name = "BottomPortContainer";
//             this.Add(bottomPortContainer);

//             inputPortViews = new List<HNGraphBasePortView>();
//             outputPortViews = new List<HNGraphBasePortView>();
//         }

//         public virtual void Initialize(HNGraphData editorData)
//         {
//             DrawNode(editorData);
//             DrawPorts(editorData);
//             SetPosition(baseNodeData.GetLayout());
//         }

//         public abstract void AddPortView(HNGraphData editorData, HNGraphBasePortView portView);

//         public abstract void RemovePortView(HNGraphData editorData, HNGraphBasePortView portView);

//         protected abstract void DrawNode(HNGraphData editorData);

//         protected abstract void DrawPorts(HNGraphData editorData);

//         public void SavePosition()
//         {
//             baseNodeData.SetLayout(GetPosition());
//         }
//     }
// }
