using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HN.Framework
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SheetElementTypeAttribute : Attribute
    {
        public SheetElementTypeAttribute(string typeName)
        {
            this.typeName = typeName;
        }


        public string typeName;
    }


    /// <summary>
    /// 表格字段类型映射工具类，提供字符串类型名到 C# 类型和 Unity 资源类型的映射
    /// </summary>
    public static class SheetType
    {
        /// <summary>
        /// 基础类型映射表，将字符串名称映射为对应的 C# 基础类型
        /// </summary>
        public static Dictionary<string, Type> BaseType = new Dictionary<string, Type>()
        {
            { "INT", typeof(int) },
            { "FLOAT", typeof(float) },
            { "STRING", typeof(string) },
        };


        /// <summary>
        /// Unity 资源类型映射表，将字符串名称映射为对应的 Unity 资源类型
        /// </summary>
        public static Dictionary<string, Type> UnityType = new Dictionary<string, Type>()
        {
            { "PREFAB", typeof(GameObject) },
            { "MATERIAL", typeof(Material) },
            { "TEXTURE", typeof(Texture) },
            { "SHADER", typeof(Shader) },
        };

    }


}
