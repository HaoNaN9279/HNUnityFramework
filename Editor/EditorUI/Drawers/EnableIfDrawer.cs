using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Drawers
{
    /// <summary>
    /// <see cref="EnableIfAttribute"/> 和 <see cref="DisableIfAttribute"/> 的 PropertyDrawer。
    /// 根据条件字段的值决定字段是否可编辑（启用或禁用）。
    /// </summary>
    [CustomPropertyDrawer(typeof(EnableIfAttribute))]
    [CustomPropertyDrawer(typeof(DisableIfAttribute))]
    public class EnableIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool conditionMet = GetConditionValue(property);
            bool shouldEnable;

            // EnableIf: 条件为 true 时启用；DisableIf: 条件为 true 时禁用
            if (fieldInfo != null && fieldInfo.GetCustomAttribute<DisableIfAttribute>() != null)
            {
                shouldEnable = !conditionMet;
            }
            else
            {
                shouldEnable = conditionMet;
            }

            bool wasEnabled = GUI.enabled;
            GUI.enabled = shouldEnable;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = wasEnabled;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private bool GetConditionValue(SerializedProperty property)
        {
            string conditionFieldName = GetConditionFieldName();
            if (string.IsNullOrEmpty(conditionFieldName))
                return true;

            // 从 SerializedProperty 的完整路径反推目标属性
            string propertyPath = property.propertyPath;
            string parentPath = propertyPath.Contains(".")
                ? propertyPath.Substring(0, propertyPath.LastIndexOf('.'))
                : "";

            string conditionPath = string.IsNullOrEmpty(parentPath)
                ? conditionFieldName
                : parentPath + "." + conditionFieldName;

            SerializedProperty conditionProp = property.serializedObject.FindProperty(conditionPath);

            if (conditionProp == null)
            {
                // 非序列化字段，用反射读取
                return GetConditionFromReflection(property.serializedObject.targetObject, conditionFieldName);
            }

            if (conditionProp.propertyType == SerializedPropertyType.Boolean)
            {
                return conditionProp.boolValue;
            }

            // 如果不是 bool 类型，返回 true（不做特殊处理）
            return true;
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
            var enableIf = fieldInfo.GetCustomAttribute<EnableIfAttribute>();
            if (enableIf != null) return enableIf.Condition;
            var disableIf = fieldInfo.GetCustomAttribute<DisableIfAttribute>();
            if (disableIf != null) return disableIf.Condition;
            return null;
        }
    }
}
