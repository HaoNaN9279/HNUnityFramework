using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Drawers
{
    /// <summary>
    /// <see cref="ShowIfAttribute"/> 和 <see cref="HideIfAttribute"/> 的 PropertyDrawer。
    /// 根据条件字段的值决定是否显示或隐藏字段。
    /// </summary>
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    [CustomPropertyDrawer(typeof(HideIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool shouldShow = ShouldShow(property);
            if (shouldShow)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool shouldShow = ShouldShow(property);
            if (!shouldShow)
            {
                return 0f;
            }
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool ShouldShow(SerializedProperty property)
        {
            string conditionFieldName = GetConditionFieldName();
            if (string.IsNullOrEmpty(conditionFieldName))
                return true;

            // 从 SerializedProperty 的完整路径反推目标属性
            // 找到同层级同名的 SerializedProperty
            string propertyPath = property.propertyPath;
            string parentPath = propertyPath.Contains(".") 
                ? propertyPath.Substring(0, propertyPath.LastIndexOf('.')) 
                : "";
            
            string conditionPath = string.IsNullOrEmpty(parentPath) 
                ? conditionFieldName 
                : parentPath + "." + conditionFieldName;

            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty conditionProp = serializedObject.FindProperty(conditionPath);
            
            if (conditionProp == null)
            {
                // 可能是非序列化字段，尝试反射读取
                return GetConditionFromReflection(serializedObject.targetObject, conditionFieldName);
            }

            bool conditionValue = conditionProp.boolValue;

            // ShowIf: 条件为 true 时显示；HideIf: 条件为 true 时隐藏
            if (fieldInfo != null && fieldInfo.GetCustomAttribute<HideIfAttribute>() != null)
            {
                return !conditionValue; // HideIf: 条件为 true 时隐藏
            }
            return conditionValue; // ShowIf: 条件为 true 时显示
        }

        private static bool GetConditionFromReflection(object targetObject, string fieldName)
        {
            if (targetObject == null) return true;
            Type type = targetObject.GetType();
            FieldInfo field = type.GetField(fieldName, 
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(bool))
            {
                return (bool)field.GetValue(targetObject);
            }

            PropertyInfo prop = type.GetProperty(fieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool) && prop.CanRead)
            {
                return (bool)prop.GetValue(targetObject, null);
            }

            return true;
        }

        private string GetConditionFieldName()
        {
            if (fieldInfo == null) return null;
            var showIf = fieldInfo.GetCustomAttribute<ShowIfAttribute>();
            if (showIf != null) return showIf.Condition;
            var hideIf = fieldInfo.GetCustomAttribute<HideIfAttribute>();
            if (hideIf != null) return hideIf.Condition;
            return null;
        }
    }
}
