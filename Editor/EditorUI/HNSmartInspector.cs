using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;
using HN.Framework.Editor.EditorUI.Descriptors;
using HN.Framework.Editor.EditorUI.Utility;

namespace HN.Framework.Editor.EditorUI
{
    /// <summary>
    /// HNSmartInspector — 智能 Inspector 主入口。
    /// 以 [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true)] 全局注册，
    /// 自动覆盖所有无专属 Editor 的 MonoBehaviour。
    /// 检测到框架自定义属性时走增强绘制路径，否则 DrawDefaultInspector() 快速返回。
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true)]
    internal class HNSmartInspector : UnityEditor.Editor
    {
        private bool _hasFrameworkAttributes;
        private bool _checkedAttributes;

        private PropertyOrderDescriptor _orderDescriptor;
        private FoldoutGroupDescriptor _foldoutDescriptor;
        private BoxGroupDescriptor _boxDescriptor;
        private TabGroupDescriptor _tabDescriptor;
        private HorizontalGroupDescriptor _horizontalDescriptor;
        private VerticalGroupDescriptor _verticalDescriptor;
        private ShowInInspectorDescriptor _showInInspectorDescriptor;
        private TitleDescriptor _titleDescriptor;
        private InfoBoxDescriptor _infoBoxDescriptor;

        // 组路径跟踪
        private readonly Dictionary<string, int> _groupFieldCount = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _groupFieldIndex = new Dictionary<string, int>();
        private string _currentGroupPath;

        private void EnsureDescriptors()
        {
            if (_titleDescriptor == null)
            {
                _titleDescriptor = new TitleDescriptor();
            }
            if (_infoBoxDescriptor == null)
            {
                _infoBoxDescriptor = new InfoBoxDescriptor();
            }
            if (_orderDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(PropertyOrderAttribute));
                _orderDescriptor = descriptors.Length > 0 ? (PropertyOrderDescriptor)descriptors[0] : new PropertyOrderDescriptor();
            }
            if (_foldoutDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(FoldoutGroupAttribute));
                _foldoutDescriptor = descriptors.Length > 0 ? (FoldoutGroupDescriptor)descriptors[0] : new FoldoutGroupDescriptor();
            }
            if (_boxDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(BoxGroupAttribute));
                _boxDescriptor = descriptors.Length > 0 ? (BoxGroupDescriptor)descriptors[0] : new BoxGroupDescriptor();
            }
            if (_tabDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(TabGroupAttribute));
                _tabDescriptor = descriptors.Length > 0 ? (TabGroupDescriptor)descriptors[0] : new TabGroupDescriptor();
            }
            if (_horizontalDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(HorizontalGroupAttribute));
                _horizontalDescriptor = descriptors.Length > 0 ? (HorizontalGroupDescriptor)descriptors[0] : new HorizontalGroupDescriptor();
            }
            if (_verticalDescriptor == null)
            {
                var descriptors = DescriptorRegistry.GetClassDescriptors(typeof(VerticalGroupAttribute));
                _verticalDescriptor = descriptors.Length > 0 ? (VerticalGroupDescriptor)descriptors[0] : new VerticalGroupDescriptor();
            }
            if (_showInInspectorDescriptor == null)
            {
                _showInInspectorDescriptor = new ShowInInspectorDescriptor();
            }
        }

        private bool HasFrameworkAttributes()
        {
            if (_checkedAttributes)
                return _hasFrameworkAttributes;

            _checkedAttributes = true;
            Type targetType = target.GetType();

            // 检查字段属性
            var fields = ReflectionCache.GetFields(targetType);
            foreach (var field in fields)
            {
                if (HasFrameworkAttribute(field))
                {
                    _hasFrameworkAttributes = true;
                    return true;
                }
            }

            // 检查方法级属性 (Button)
            var methods = ReflectionCache.GetMethods(targetType);
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<ButtonAttribute>() != null)
                {
                    _hasFrameworkAttributes = true;
                    return true;
                }
            }

            // 检查类级属性
            if (Attribute.IsDefined(targetType, typeof(ShowInInspectorAttribute)))
            {
                _hasFrameworkAttributes = true;
                return true;
            }

            return false;
        }

        private static bool HasFrameworkAttribute(FieldInfo field)
        {
            if (field.GetCustomAttribute<ReadOnlyAttribute>() != null) return true;
            if (field.GetCustomAttribute<ShowIfAttribute>() != null) return true;
            if (field.GetCustomAttribute<HideIfAttribute>() != null) return true;
            if (field.GetCustomAttribute<EnableIfAttribute>() != null) return true;
            if (field.GetCustomAttribute<DisableIfAttribute>() != null) return true;
            if (field.GetCustomAttribute<TitleAttribute>() != null) return true;
            if (field.GetCustomAttribute<InfoBoxAttribute>() != null) return true;
            if (field.GetCustomAttribute<PropertyOrderAttribute>() != null) return true;
            if (field.GetCustomAttribute<FoldoutGroupAttribute>() != null) return true;
            if (field.GetCustomAttribute<BoxGroupAttribute>() != null) return true;
            if (field.GetCustomAttribute<TabGroupAttribute>() != null) return true;
            if (field.GetCustomAttribute<HorizontalGroupAttribute>() != null) return true;
            if (field.GetCustomAttribute<VerticalGroupAttribute>() != null) return true;
            if (field.GetCustomAttribute<RequiredAttribute>() != null) return true;
            if (field.GetCustomAttribute<OnValueChangedAttribute>() != null) return true;
            if (field.GetCustomAttribute<ValidateInputAttribute>() != null) return true;
            if (field.GetCustomAttribute<ShowInInspectorAttribute>() != null) return true;
            return false;
        }

        public override void OnInspectorGUI()
        {
            // 快速路径：没有框架属性时直接走默认 Inspector
            if (!HasFrameworkAttributes())
            {
                DrawDefaultInspector();
                return;
            }

            EnsureDescriptors();
            serializedObject.Update();

            // 类级 BeforeClass（仅 IClassDescriptor 有此方法）
            _orderDescriptor.BeforeClass();
            _foldoutDescriptor.BeforeClass();
            _boxDescriptor.BeforeClass();
            _tabDescriptor.BeforeClass();
            _horizontalDescriptor.BeforeClass();
            _verticalDescriptor.BeforeClass();

            // 收集所有可序列化属性
            var propertyPaths = new List<string>();
            var property = serializedObject.GetIterator();
            property.NextVisible(true);
            while (property.NextVisible(false))
            {
                propertyPaths.Add(property.propertyPath);
            }

            // 按 PropertyOrder 排序
            var orderedPaths = _orderDescriptor.GetOrderedPropertyPaths(serializedObject).ToList();

            // 绘制标签页栏（如果有标签页组）
            string selectedTab = _tabDescriptor.DrawTabBar();

            // 准备组路径统计
            BuildGroupMaps(orderedPaths);

            // 逐字段绘制
            foreach (string path in orderedPaths)
            {
                var prop = serializedObject.FindProperty(path);
                if (prop == null) continue;

                // 标签页过滤
                if (!string.IsNullOrEmpty(selectedTab) && !_tabDescriptor.IsInSelectedTab(path))
                    continue;

                // 获取字段的 Framework 属性
                FieldInfo field = GetFieldFromPath(prop);
                if (field == null)
                {
                    EditorGUILayout.PropertyField(prop, true);
                    continue;
                }

                // 组处理：开始新组
                HandleGroupBegin(field, prop);

                // 字段级 BeforeField
                HandleBeforeField(field, prop);

                // 绘制字段（PropertyDrawer 由 Unity 自动处理）
                EditorGUILayout.PropertyField(prop, true);

                // 字段级 AfterField
                HandleAfterField(field, prop);

                // 组处理：结束当前组
                HandleGroupEnd(field, prop);
            }

            // 绘制 [Button] 方法
            DrawButtonMethods();

            // 绘制 [ShowInInspector] 非序列化字段
            DrawShowInInspectorFields();

            // 类级 AfterClass（仅 IClassDescriptor 有此方法）
            _orderDescriptor.AfterClass();
            _foldoutDescriptor.AfterClass();
            _boxDescriptor.AfterClass();
            _tabDescriptor.AfterClass();
            _horizontalDescriptor.AfterClass();
            _verticalDescriptor.AfterClass();

            serializedObject.ApplyModifiedProperties();
        }

        private void BuildGroupMaps(List<string> orderedPaths)
        {
            _groupFieldCount.Clear();
            _groupFieldIndex.Clear();

            foreach (string path in orderedPaths)
            {
                FieldInfo field = GetFieldFromPath(serializedObject.FindProperty(path));
                if (field == null) continue;

                string groupPath = GetGroupPath(field);
                if (!string.IsNullOrEmpty(groupPath))
                {
                    if (!_groupFieldCount.ContainsKey(groupPath))
                        _groupFieldCount[groupPath] = 0;
                    _groupFieldCount[groupPath]++;
                }
            }

            foreach (string path in orderedPaths)
            {
                FieldInfo field = GetFieldFromPath(serializedObject.FindProperty(path));
                if (field == null) continue;

                string groupPath = GetGroupPath(field);
                if (!string.IsNullOrEmpty(groupPath))
                {
                    if (!_groupFieldIndex.ContainsKey(groupPath))
                        _groupFieldIndex[groupPath] = 0;
                }
            }
        }

        private string GetGroupPath(FieldInfo field)
        {
            var foldout = field.GetCustomAttribute<FoldoutGroupAttribute>();
            if (foldout != null) return "foldout:" + foldout.Path;

            var box = field.GetCustomAttribute<BoxGroupAttribute>();
            if (box != null) return "box:" + box.Path;

            var tab = field.GetCustomAttribute<TabGroupAttribute>();
            if (tab != null) return "tab:" + tab.Path;

            var h = field.GetCustomAttribute<HorizontalGroupAttribute>();
            if (h != null) return "h:" + h.Path;

            var v = field.GetCustomAttribute<VerticalGroupAttribute>();
            if (v != null) return "v:" + v.Path;

            return null;
        }

        private void HandleGroupBegin(FieldInfo field, SerializedProperty prop)
        {
            string groupType = GetGroupType(field);
            if (groupType == null) return;

            switch (groupType)
            {
                case "foldout":
                    var foldoutAttr = field.GetCustomAttribute<FoldoutGroupAttribute>();
                    if (foldoutAttr != null)
                    {
                        bool expanded = _foldoutDescriptor.BeginFoldoutGroup(foldoutAttr.Path);
                        if (!expanded)
                        {
                            // 如果折叠组未展开，跳过组内所有后续字段
                            GUI.enabled = false;
                        }
                    }
                    break;
                case "box":
                    var boxAttr = field.GetCustomAttribute<BoxGroupAttribute>();
                    if (boxAttr != null)
                        _boxDescriptor.BeginBoxGroup(boxAttr.Path);
                    break;
                case "h":
                    _horizontalDescriptor.BeginHorizontalGroup();
                    break;
                case "v":
                    _verticalDescriptor.BeginVerticalGroup();
                    break;
            }
        }

        private void HandleGroupEnd(FieldInfo field, SerializedProperty prop)
        {
            string groupType = GetGroupType(field);
            if (groupType == null) return;

            switch (groupType)
            {
                case "foldout":
                    _foldoutDescriptor.EndFoldoutGroup();
                    GUI.enabled = true;
                    break;
                case "box":
                    _boxDescriptor.EndBoxGroup();
                    break;
                case "h":
                    _horizontalDescriptor.EndHorizontalGroup();
                    break;
                case "v":
                    _verticalDescriptor.EndVerticalGroup();
                    break;
            }
        }

        private string GetGroupType(FieldInfo field)
        {
            if (field.GetCustomAttribute<FoldoutGroupAttribute>() != null) return "foldout";
            if (field.GetCustomAttribute<BoxGroupAttribute>() != null) return "box";
            if (field.GetCustomAttribute<TabGroupAttribute>() != null) return "tab";
            if (field.GetCustomAttribute<HorizontalGroupAttribute>() != null) return "h";
            if (field.GetCustomAttribute<VerticalGroupAttribute>() != null) return "v";
            return null;
        }

        private void HandleBeforeField(FieldInfo field, SerializedProperty prop)
        {
            // Title
            var titleAttr = field.GetCustomAttribute<TitleAttribute>();
            if (titleAttr != null)
            {
                Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                _titleDescriptor.BeforeField(position, prop, titleAttr, new GUIContent(prop.displayName));
            }

            // InfoBox
            var infoBoxAttr = field.GetCustomAttribute<InfoBoxAttribute>();
            if (infoBoxAttr != null)
            {
                float helpBoxHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(infoBoxAttr.Message), EditorGUIUtility.currentViewWidth);
                Rect position = EditorGUILayout.GetControlRect(true, helpBoxHeight);
                _infoBoxDescriptor.BeforeField(position, prop, infoBoxAttr, new GUIContent(prop.displayName));
            }

            // ShowInInspector
            if (field.GetCustomAttribute<ShowInInspectorAttribute>() != null)
            {
                object value = field.GetValue(target);
                EditorGUILayout.LabelField(field.Name, value?.ToString() ?? "null");
            }

            // IPropertyDescriptor 链
            var allAttrs = field.GetCustomAttributes(true);
            foreach (var attr in allAttrs)
            {
                Type attrType = attr.GetType();
                var descriptors = DescriptorRegistry.GetPropertyDescriptors(attrType);
                foreach (var descriptor in descriptors)
                {
                    if (descriptor is TitleDescriptor || descriptor is InfoBoxDescriptor || descriptor is ShowInInspectorDescriptor)
                        continue; // 已单独处理
                    Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                    descriptor.BeforeField(position, prop, (Attribute)attr, new GUIContent(prop.displayName));
                }
            }
        }

        private void HandleAfterField(FieldInfo field, SerializedProperty prop)
        {
            var allAttrs = field.GetCustomAttributes(true);
            foreach (var attr in allAttrs)
            {
                Type attrType = attr.GetType();
                var descriptors = DescriptorRegistry.GetPropertyDescriptors(attrType);
                foreach (var descriptor in descriptors)
                {
                    if (descriptor is TitleDescriptor || descriptor is InfoBoxDescriptor || descriptor is ShowInInspectorDescriptor)
                        continue;
                    Rect position = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                    descriptor.AfterField(position, prop, (Attribute)attr, new GUIContent(prop.displayName));
                }
            }
        }

        private void DrawButtonMethods()
        {
            Type targetType = target.GetType();
            var methods = ReflectionCache.GetMethods(targetType);

            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null) continue;

                string buttonName = string.IsNullOrEmpty(buttonAttr.ButtonName)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : buttonAttr.ButtonName;

                float buttonHeight = buttonAttr.ButtonHeight > 0
                    ? buttonAttr.ButtonHeight
                    : HNSmartInspectorStyles.DefaultButtonHeight;

                if (GUILayout.Button(buttonName, GUILayout.Height(buttonHeight)))
                {
                    method.Invoke(target, null);
                    if (buttonAttr.DirtyOnClick)
                    {
                        EditorUtility.SetDirty(target as MonoBehaviour);
                    }
                }
            }
        }

        private void DrawShowInInspectorFields()
        {
            var fields = _showInInspectorDescriptor.GetShowInInspectorFields(target);
            if (fields.Length == 0) return;

            EditorGUILayout.Space(4);

            foreach (var (name, value) in fields)
            {
                string displayValue = value?.ToString() ?? "null";
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(name), displayValue);
            }
        }

        private FieldInfo GetFieldFromPath(SerializedProperty prop)
        {
            if (prop == null) return null;
            Type targetType = target.GetType();
            var fields = ReflectionCache.GetFields(targetType);
            string fieldName = prop.name;

            // 尝试直接匹配
            foreach (var f in fields)
            {
                if (f.Name == fieldName)
                    return f;
            }

            // 尝试数组路径匹配
            int arrayIndex = fieldName.IndexOf("Array.data[");
            if (arrayIndex > 0)
            {
                string baseName = fieldName.Substring(0, arrayIndex);
                foreach (var f in fields)
                {
                    if (f.Name == baseName)
                        return f;
                }
            }

            return null;
        }
    }
}
