using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Editor
{
    public class HNUndoableObject : ScriptableObject
    {
        public virtual void RecordObject(string actionName)
        {
            Undo.RecordObject(this, actionName);
        }
    }
}
