using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="FoldoutGroupAttribute"/> 的类级处理器。
    /// 将同组的字段放入一个可折叠的面板中，管理折叠状态。
    /// </summary>
    public class FoldoutGroupDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(FoldoutGroupAttribute) };
        public int Order => 100;

        private readonly Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

        private string _currentGroupPath;

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes)
        {
            if (attributes.Length > 0 && attributes[0] is FoldoutGroupAttribute attr)
            {
                _currentGroupPath = attr.Path;
            }
        }

        public void BeforeClass() { }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 在 HNSmartInspector 中调用，绘制折叠组的外层。
        /// 返回 true 表示组已展开，应该绘制组内字段。
        /// </summary>
        public bool BeginFoldoutGroup(string groupPath)
        {
            if (!_foldoutStates.ContainsKey(groupPath))
            {
                _foldoutStates[groupPath] = true; // 默认展开
            }

            _foldoutStates[groupPath] = EditorGUILayout.Foldout(_foldoutStates[groupPath], GetGroupDisplayName(groupPath), true);
            EditorGUI.indentLevel++;

            return _foldoutStates[groupPath];
        }

        /// <summary>
        /// 结束折叠组。
        /// </summary>
        public void EndFoldoutGroup()
        {
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }

        private static string GetGroupDisplayName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Group";
            string[] parts = path.Split('/');
            return parts[parts.Length - 1];
        }

        /// <summary>
        /// 检查指定组是否已展开。
        /// </summary>
        public bool IsGroupExpanded(string groupPath)
        {
            return _foldoutStates.TryGetValue(groupPath, out bool expanded) && expanded;
        }

        /// <summary>
        /// 获取当前所有组的路径集合（含已注册的折叠组属性）。
        /// </summary>
        public IEnumerable<string> GetRegisteredGroups()
        {
            return _foldoutStates.Keys;
        }
    }
}
