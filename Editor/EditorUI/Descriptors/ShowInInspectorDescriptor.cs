using System;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="ShowInInspectorAttribute"/> 的字段级描述器。
    /// 强制在 Inspector 中显示非序列化字段（只读方式显示）。
    /// </summary>
    public class ShowInInspectorDescriptor : IPropertyDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(ShowInInspectorAttribute) };

        public void BeforeField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label) { }

        public void AfterField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label) { }

        /// <summary>
        /// 获取标记了 [ShowInInspector] 的字段信息，用于 HNSmartInspector 额外绘制。
        /// 返回字段名和当前值的字符串表示。
        /// </summary>
        public (string name, object value)[] GetShowInInspectorFields(object target)
        {
            if (target == null) return Array.Empty<(string, object)>();

            var type = target.GetType();
            var fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

            var results = new System.Collections.Generic.List<(string, object)>();
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(ShowInInspectorAttribute)))
                {
                    results.Add((field.Name, field.GetValue(target)));
                }

                var properties = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                foreach (var prop in properties)
                {
                    if (Attribute.IsDefined(prop, typeof(ShowInInspectorAttribute)) && prop.CanRead)
                    {
                        results.Add((prop.Name, prop.GetValue(target, null)));
                    }
                }
            }

            return results.ToArray();
        }
    }
}
