using System;

namespace HN.Framework.Editor
{
    /// <summary>
    /// 标记 Sheet 字段类型的特性，指定类型名称。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SheetFieldTypeAttribute : Attribute
    {
        /// <summary>
        /// 使用指定的类型名称创建特性实例。
        /// </summary>
        /// <param name="typeName">字段类型名称。</param>
        public SheetFieldTypeAttribute(string typeName)
        {
            this.typeName = typeName;
        }

        /// <summary>
        /// 字段类型名称。
        /// </summary>
        public string typeName;
    }
}
