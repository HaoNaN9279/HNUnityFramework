using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Drawers
{
    /// <summary>
    /// <see cref="OnValueChangedAttribute"/> 的 PropertyDrawer。
    /// 当字段值发生变化时，调用指定的回调方法。
    /// </summary>
    [CustomPropertyDrawer(typeof(OnValueChangedAttribute))]
    public class OnValueChangedDrawer : PropertyDrawer
    {
        private object _lastValue;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(position, property, label, true);

            if (EditorGUI.EndChangeCheck())
            {
                var attr = (OnValueChangedAttribute)attribute;
                InvokeMethod(property.serializedObject.targetObject, attr.MethodName);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private static void InvokeMethod(object target, string methodName)
        {
            if (target == null || string.IsNullOrEmpty(methodName)) return;

            Type type = target.GetType();
            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (method != null)
            {
                method.Invoke(target, null);
            }
            else
            {
                Debug.LogWarning($"[OnValueChanged] Method '{methodName}' not found on {type.Name}");
            }
        }
    }
}