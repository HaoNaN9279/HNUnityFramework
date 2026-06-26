using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor
{
    public class HNUndoableObject : ScriptableObject
    {
        /// <summary>
        /// 记录对象状态以便撤销操作
        /// </summary>
        /// <param name="actionName">操作名称</param>
        public virtual void RecordObject(string actionName)
        {
            Undo.RecordObject(this, actionName);
        }
    }
}
