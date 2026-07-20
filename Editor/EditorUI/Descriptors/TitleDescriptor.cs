using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="TitleAttribute"/> 的字段级描述器。
    /// 在字段上方渲染标题文本（含可选副标题、粗体、水平线）。
    /// </summary>
    public class TitleDescriptor : IPropertyDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(TitleAttribute) };

        public void BeforeField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label)
        {
            var titleAttr = (TitleAttribute)attribute;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.label);
            if (titleAttr.Bold)
            {
                titleStyle.fontStyle = FontStyle.Bold;
            }
            titleStyle.fontSize = 12;

            Rect titleRect = new Rect(position.x, position.y - 18, position.width, 18);
            EditorGUI.LabelField(titleRect, titleAttr.Title, titleStyle);

            if (!string.IsNullOrEmpty(titleAttr.Subtitle))
            {
                Rect subtitleRect = new Rect(position.x, titleRect.yMax - 2, position.width, 14);
                GUIStyle subtitleStyle = new GUIStyle(EditorStyles.miniLabel);
                subtitleStyle.normal.textColor = Color.gray;
                EditorGUI.LabelField(subtitleRect, titleAttr.Subtitle, subtitleStyle);
            }

            if (titleAttr.HorizontalLine)
            {
                float lineY = (string.IsNullOrEmpty(titleAttr.Subtitle) ? titleRect.yMax : titleRect.yMax + 12) + 2;
                Rect lineRect = new Rect(position.x, lineY, position.width, 1);
                EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.4f));
            }
        }

        public void AfterField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label)
        {
            // Title 只在字段前绘制，不需要 AfterField 逻辑
        }
    }
}
