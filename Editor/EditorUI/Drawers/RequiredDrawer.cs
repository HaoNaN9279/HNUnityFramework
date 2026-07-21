using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Drawers
{
    /// <summary>
    /// <see cref="RequiredAttribute"/> 的 PropertyDrawer。
    /// 当字段值为 null 时在字段右侧显示红色警告图标。
    /// 支持 UnityEngine.Object 引用类型和 string 类型的空值检测。
    /// </summary>
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    public class RequiredDrawer : PropertyDrawer
    {
        private const float WarningWidth = 20f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequiredAttribute requiredAttr = (RequiredAttribute)attribute;
            bool isNull = IsNullValue(property);

            Rect fieldRect = position;
            if (isNull)
            {
                fieldRect.width -= WarningWidth;
            }

            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (isNull)
            {
                Rect warningRect = new Rect(position.x + position.width - WarningWidth, position.y, WarningWidth, EditorGUIUtility.singleLineHeight);
                string tooltip = string.IsNullOrEmpty(requiredAttr.ErrorMessage)
                    ? "This field is required."
                    : requiredAttr.ErrorMessage;
                GUIContent warningIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
                warningIcon.tooltip = tooltip;
                GUI.Label(warningRect, warningIcon);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private static bool IsNullValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                case SerializedPropertyType.String:
                    return string.IsNullOrEmpty(property.stringValue);
                default:
                    return false;
            }
        }
    }
}