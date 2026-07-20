using System;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// 字段级属性描述器接口。
    /// 实现此接口并在 <see cref="DescriptorRegistry"/> 中注册，可在字段绘制前后插入自定义绘制逻辑。
    /// </summary>
    public interface IPropertyDescriptor
    {
        /// <summary>
        /// 此描述器支持处理的属性类型列表。
        /// 用于在运行时快速匹配字段是否包含需要此描述器处理的属性。
        /// </summary>
        Type[] SupportedAttributeTypes { get; }

        /// <summary>
        /// 在字段绘制之前调用。可用于在此字段上方绘制装饰元素。
        /// </summary>
        /// <param name="position">字段矩形区域</param>
        /// <param name="property">序列化属性</param>
        /// <param name="attribute">触发此描述器的属性实例</param>
        /// <param name="label">字段标签</param>
        void BeforeField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label);

        /// <summary>
        /// 在字段绘制之后调用。可用于在此字段下方绘制附加信息。
        /// </summary>
        /// <param name="position">字段矩形区域</param>
        /// <param name="property">序列化属性</param>
        /// <param name="attribute">触发此描述器的属性实例</param>
        /// <param name="label">字段标签</param>
        void AfterField(Rect position, SerializedProperty property, Attribute attribute, GUIContent label);
    }
}
