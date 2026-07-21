using System;
using UnityEditor;
using UnityEngine;

namespace HN.Framework.Editor.EditorUI.Descriptors
{
    /// <summary>
    /// 类级属性处理器接口。
    /// 实现此接口并在 <see cref="DescriptorRegistry"/> 中注册，可在类级别控制 Inspector 布局（如折叠组、框线组、标签页等）。
    /// 多个 IClassDescriptor 按 <see cref="Order"/> 升序执行。
    /// </summary>
    public interface IClassDescriptor
    {
        /// <summary>
        /// 此描述器支持处理的属性类型列表。
        /// </summary>
        Type[] SupportedAttributeTypes { get; }

        /// <summary>
        /// 执行顺序。数值越小越先执行。
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 在类级别绘制开始之前调用。用于启动分组布局（如 BeginVertical）。
        /// </summary>
        /// <param name="property">当前正在绘制的序列化属性</param>
        /// <param name="attributes">此描述器涉及的所有属性列表</param>
        void OnGroupBegin(SerializedProperty property, Attribute[] attributes);

        /// <summary>
        /// 在类级别绘制开始之前调用（所有字段绘制之前）。
        /// </summary>
        void BeforeClass();

        /// <summary>
        /// 在类级别绘制结束之后调用（所有字段绘制之后）。
        /// </summary>
        void AfterClass();

        /// <summary>
        /// 在类级别绘制结束之后调用。用于结束分组布局（如 EndVertical）。
        /// </summary>
        /// <param name="property">当前正在绘制的序列化属性</param>
        /// <param name="attributes">此描述器涉及的所有属性列表</param>
        void OnGroupEnd(SerializedProperty property, Attribute[] attributes);
    }
}
