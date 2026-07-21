using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="InfoBoxAttribute"/> 的字段级描述器。
    /// 在字段上方渲染一个信息/警告/错误消息框，支持条件显示。
    /// </summary>
    public class InfoBoxDescriptor : IPropertyDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(InfoBoxAttribute) };

        public void BeforeField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label)
        {
            var infoBoxAttr = (InfoBoxAttribute)attribute;

            // 检查条件显示
            if (!string.IsNullOrEmpty(infoBoxAttr.VisibleIf))
            {
                SerializedProperty conditionProp = property.serializedObject.FindProperty(infoBoxAttr.VisibleIf);
                if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean)
                {
                    if (!conditionProp.boolValue)
                        return;
                }
            }

            MessageType messageType = MapMessageType(infoBoxAttr.Type);
            float helpBoxHeight = EditorStyles.helpBox.CalcHeight(new GUIContent(infoBoxAttr.Message), position.width);

            Rect helpBoxRect = new Rect(position.x, position.y - helpBoxHeight - 4, position.width, helpBoxHeight);
            EditorGUI.HelpBox(helpBoxRect, infoBoxAttr.Message, messageType);
        }

        public void AfterField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label)
        {
            // InfoBox 只在字段前绘制
        }

        private static MessageType MapMessageType(InfoMessageType type)
        {
            switch (type)
            {
                case InfoMessageType.Info: return MessageType.Info;
                case InfoMessageType.Warning: return MessageType.Warning;
                case InfoMessageType.Error: return MessageType.Error;
                default: return MessageType.None;
            }
        }
    }
}
