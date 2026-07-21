using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Drawers
{
    /// <summary>
    /// <see cref="ValidateInputAttribute"/> 的 PropertyDrawer。
    /// 调用指定的校验方法验证字段值，失败时显示错误消息。
    /// 校验方法签名：bool MethodName(object value, out string errorMessage)
    /// </summary>
    [CustomPropertyDrawer(typeof(ValidateInputAttribute))]
    public class ValidateInputDrawer : PropertyDrawer
    {
        private const float ErrorHeight = 16f;
        private string _errorMessage;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (ValidateInputAttribute)attribute;
            Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, label, true));

            EditorGUI.PropertyField(fieldRect, property, label, true);

            // 执行校验
            if (Validate(property, attr, out string error))
            {
                _errorMessage = null;
            }
            else
            {
                _errorMessage = error ?? attr.Message ?? "Validation failed.";
            }

            // 显示错误消息
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                Rect errorRect = new Rect(position.x, fieldRect.yMax, position.width, ErrorHeight);
                Color originalColor = GUI.color;
                GUI.color = Color.red;
                EditorGUI.LabelField(errorRect, _errorMessage, EditorStyles.miniLabel);
                GUI.color = originalColor;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = EditorGUI.GetPropertyHeight(property, label, true);
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                return baseHeight + ErrorHeight + 2f;
            }
            return baseHeight;
        }

        private static bool Validate(SerializedProperty property, ValidateInputAttribute attr, out string error)
        {
            error = null;
            if (attr == null || string.IsNullOrEmpty(attr.MethodName))
                return true;

            object target = property.serializedObject.targetObject;
            Type type = target.GetType();
            MethodInfo method = type.GetMethod(attr.MethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method == null)
            {
                error = $"Validation method '{attr.MethodName}' not found.";
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            object[] args;

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object))
            {
                args = new object[] { GetPropertyValue(property) };
            }
            else if (parameters.Length == 2 && parameters[0].ParameterType == typeof(object) && parameters[1].ParameterType == typeof(string).MakeByRefType())
            {
                args = new object[] { GetPropertyValue(property), null };
            }
            else
            {
                error = $"Validation method '{attr.MethodName}' has an unsupported signature.";
                return false;
            }

            bool result = (bool)method.Invoke(target, args);

            if (args.Length == 2 && !result)
            {
                error = args[1] as string;
            }

            return result;
        }

        private static object GetPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: return property.intValue;
                case SerializedPropertyType.Float: return property.floatValue;
                case SerializedPropertyType.Boolean: return property.boolValue;
                case SerializedPropertyType.String: return property.stringValue;
                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue;
                case SerializedPropertyType.Enum: return property.enumValueIndex;
                case SerializedPropertyType.Vector2: return property.vector2Value;
                case SerializedPropertyType.Vector3: return property.vector3Value;
                case SerializedPropertyType.Color: return property.colorValue;
                default: return null;
            }
        }
    }
}