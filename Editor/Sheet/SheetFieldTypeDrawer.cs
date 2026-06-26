using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace HN.Framework.Editor
{
    /// <summary>
    /// Sheet 字段类型绘制器，负责根据类型名称动态创建对应的字段编辑器。
    /// </summary>
    public class SheetFieldTypeDrawer
    {
        /// <summary>
        /// 构造函数，扫描并注册所有实现了 <see cref="ISheetFieldTypeEditor"/> 的字段类型编辑器。
        /// </summary>
        /// <param name="elementsProperty">元素数组的 SerializedProperty。</param>
        public SheetFieldTypeDrawer(SerializedProperty elementsProperty)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => typeof(ISheetFieldTypeEditor).IsAssignableFrom(t) && !t.IsInterface)
                        .Select(t => new { Type = t, Attr = t.GetCustomAttribute<SheetFieldTypeAttribute>() })
                        .Where(x => x.Attr != null);
            foreach (var item in types)
            {
                drawerDict[item.Attr.typeName] = item.Type;
            }
            typeNameList = drawerDict.Keys.ToList();
            this.elementsProperty = elementsProperty;
        }

        /// <summary>
        /// 根据类型名称绘制对应的字段编辑器。
        /// </summary>
        /// <param name="typeName">字段类型名称。</param>
        /// <param name="elementId">当前元素的索引。</param>
        /// <param name="value">当前字段的字符串值。</param>
        /// <returns>绘制的 VisualElement，如果类型未注册则返回 null。</returns>
        public VisualElement DrawField(string typeName, int elementId, string value)
        {
            if (drawerDict.TryGetValue(typeName, out var type))
            {
                var editor = (ISheetFieldTypeEditor)Activator.CreateInstance(type);
                return editor.DrawField(elementsProperty, elementId, value);
            }
            return null;
        }


        /// <summary>
        /// 获取类型名称到编辑器类型的映射字典。
        /// </summary>
        public IReadOnlyDictionary<string, Type> DrawerDict => drawerDict;
        
        /// <summary>
        /// 获取已注册的类型名称列表。
        /// </summary>
        public List<string> TypeNameList => typeNameList;
        
        private Dictionary<string, Type> drawerDict = new();
        private List<string> typeNameList;
        private SerializedProperty elementsProperty;
    }
}
