using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using HN.Framework.Unity.EditorUI.Attributes;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// <see cref="PropertyOrderAttribute"/> 的类级处理器。
    /// 根据 [PropertyOrder] 的 Order 值重排字段绘制顺序。
    /// </summary>
    public class PropertyOrderDescriptor : IClassDescriptor
    {
        public Type[] SupportedAttributeTypes => new[] { typeof(PropertyOrderAttribute) };
        public int Order => -100; // 最先执行

        private List<(int order, string propertyPath)> _orderedProperties;

        public void OnGroupBegin(SerializedProperty property, Attribute[] attributes) { }

        public void BeforeClass()
        {
            _orderedProperties = null;
        }

        public void AfterClass() { }

        public void OnGroupEnd(SerializedProperty property, Attribute[] attributes) { }

        /// <summary>
        /// 获取按 [PropertyOrder] 排序后的属性列表。
        /// 无 PropertyOrder 的属性按默认顺序排在最后。
        /// </summary>
        public IEnumerable<string> GetOrderedPropertyPaths(SerializedObject serializedObject)
        {
            if (_orderedProperties == null)
            {
                BuildOrderedList(serializedObject);
            }
            return _orderedProperties.Select(x => x.propertyPath);
        }

        private void BuildOrderedList(SerializedObject serializedObject)
        {
            _orderedProperties = new List<(int, string)>();
            var unordered = new List<string>();

            var property = serializedObject.GetIterator();
            property.NextVisible(true);

            while (property.NextVisible(false))
            {
                string path = property.propertyPath;
                var targetType = serializedObject.targetObject.GetType();
                var field = targetType.GetField(path,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    var orderAttr = (PropertyOrderAttribute)Attribute.GetCustomAttribute(field, typeof(PropertyOrderAttribute));
                    if (orderAttr != null)
                    {
                        _orderedProperties.Add((orderAttr.Order, path));
                        continue;
                    }
                }
                unordered.Add(path);
            }

            _orderedProperties = _orderedProperties.OrderBy(x => x.order).ToList();
            // 无顺序的属性排在最后
            foreach (var path in unordered)
            {
                _orderedProperties.Add((int.MaxValue, path));
            }
        }
    }
}
