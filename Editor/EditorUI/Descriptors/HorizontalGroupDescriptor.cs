using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="HorizontalGroupAttribute"/> 的类级处理器。
    /// 将同组的字段水平排列在一行中。
    /// </summary>
    public class HorizontalGroupDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(HorizontalGroupAttribute) };
        public int Order => 300;

        private string _currentGroupPath;

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes)
        {
            if (attributes.Length > 0 && attributes[0] is HorizontalGroupAttribute attr)
            {
                _currentGroupPath = attr.Path;
            }
        }

        public void BeforeClass() { }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 开始水平布局组。
        /// </summary>
        public void BeginHorizontalGroup()
        {
            EditorGUILayout.BeginHorizontal();
        }

        /// <summary>
        /// 结束水平布局组。
        /// </summary>
        public void EndHorizontalGroup()
        {
            EditorGUILayout.EndHorizontal();
        }
    }
}
