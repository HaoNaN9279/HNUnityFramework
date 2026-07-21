using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="BoxGroupAttribute"/> 的类级处理器。
    /// 将同组的字段放入一个带框线的区域中。
    /// </summary>
    public class BoxGroupDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(BoxGroupAttribute) };
        public int Order => 200;

        private string _currentGroupPath;

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes)
        {
            if (attributes.Length > 0 && attributes[0] is BoxGroupAttribute attr)
            {
                _currentGroupPath = attr.Path;
            }
        }

        public void BeforeClass() { }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 开始一个带框线的分组区域。
        /// </summary>
        public void BeginBoxGroup(string groupPath)
        {
            string displayName = GetGroupDisplayName(groupPath);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (!string.IsNullOrEmpty(displayName))
            {
                EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);
            }
        }

        /// <summary>
        /// 结束框线分组区域。
        /// </summary>
        public void EndBoxGroup()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static string GetGroupDisplayName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            string[] parts = path.Split('/');
            return parts[parts.Length - 1];
        }
    }
}
