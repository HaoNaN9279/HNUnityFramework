// using System;
// using System.Collections;
// using System.Collections.Generic;
// using HN.Serialize;
// using UnityEngine;

// namespace HN.Graph.Editor
// {
//     [Serializable]
//     public abstract class HNGraphBaseNode : IDisposable, IPositionable
//     {
//         public string Guid => guid;


//         [SerializeField]
//         protected string guid;

//         [SerializeField]
//         protected Rect layout;


//         public HNGraphBaseNode()
//         {
//         }

//         public virtual void Initialize(Vector2 position)
//         {
//             guid = HNGraphUtils.NewGuid();
//             SetLayout(new Rect(position, Vector2.zero));
//         }

//         public abstract void AddInputPort(HNGraphData editorData, HNGraphBasePort port);

//         public abstract void AddOutputPort(HNGraphData editorData, HNGraphBasePort port);

//         public abstract void RemoveInputPort(HNGraphData editorData, HNGraphBasePort port);

//         public abstract void RemoveOutputPort(HNGraphData editorData, HNGraphBasePort port);

//         public Rect GetLayout()
//         {
//             return this.layout;
//         }

//         public void SetLayout(Rect layout)
//         {
//             this.layout = layout;
//         }

//         public virtual void Dispose()
//         {
//         }
//     }
// }
