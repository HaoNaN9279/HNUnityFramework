using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="TabGroupAttribute"/> 的类级处理器。
    /// 将字段组织到标签页中，根据路径首段分组。
    /// </summary>
    public class TabGroupDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(TabGroupAttribute) };
        public int Order => 50;

        private string _selectedTab;
        private readonly List<string> _tabNames = new List<string>();
        private readonly Dictionary<string, List<string>> _tabFields = new Dictionary<string, List<string>>();

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes)
        {
            if (attributes.Length > 0 && attributes[0] is TabGroupAttribute attr)
            {
                string tabName = GetTabName(attr.Path);
                if (!_tabNames.Contains(tabName))
                {
                    _tabNames.Add(tabName);
                }
                if (!_tabFields.ContainsKey(tabName))
                {
                    _tabFields[tabName] = new List<string>();
                }
                _tabFields[tabName].Add(property.propertyPath);
            }
        }

        public void BeforeClass() { }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 绘制标签页选择栏。返回当前选中的标签页名。
        /// </summary>
        public string DrawTabBar()
        {
            if (_tabNames.Count == 0) return null;

            if (string.IsNullOrEmpty(_selectedTab) || !_tabNames.Contains(_selectedTab))
            {
                _selectedTab = _tabNames[0];
            }

            EditorGUILayout.BeginHorizontal();
            foreach (var tab in _tabNames)
            {
                bool isSelected = (tab == _selectedTab);
                GUI.enabled = !isSelected;
                if (GUILayout.Button(tab, EditorStyles.toolbarButton))
                {
                    _selectedTab = tab;
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);

            return _selectedTab;
        }

        /// <summary>
        /// 判断指定属性是否属于当前选中的标签页。
        /// </summary>
        public bool IsInSelectedTab(string propertyPath)
        {
            if (_tabFields.Count == 0) return true;
            if (string.IsNullOrEmpty(_selectedTab)) return true;

            if (_tabFields.TryGetValue(_selectedTab, out var fields))
            {
                return fields.Contains(propertyPath);
            }
            return false;
        }

        private static string GetTabName(string path)
        {
            if (string.IsNullOrEmpty(path)) return "Default";
            return path.Split('/')[0];
        }
    }
}
