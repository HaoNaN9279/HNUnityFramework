using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="VerticalGroupAttribute"/> 的类级处理器。
    /// 将同组的字段垂直排列。
    /// </summary>
    public class VerticalGroupDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(VerticalGroupAttribute) };
        public int Order => 400;

        private string _currentGroupPath;

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes)
        {
            if (attributes.Length > 0 && attributes[0] is VerticalGroupAttribute attr)
            {
                _currentGroupPath = attr.Path;
            }
        }

        public void BeforeClass() { }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 开始垂直布局组。
        /// </summary>
        public void BeginVerticalGroup()
        {
            EditorGUILayout.BeginVertical();
        }

        /// <summary>
        /// 结束垂直布局组。
        /// </summary>
        public void EndVerticalGroup()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }
}
