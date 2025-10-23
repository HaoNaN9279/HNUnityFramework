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


    public static class SheetType
    {
        public static Dictionary<string, Type> BaseType = new Dictionary<string, Type>()
        {
            { "INT", typeof(int) },
            { "FLOAT", typeof(float) },
            { "STRING", typeof(string) },
        };


        public static Dictionary<string, Type> UnityType = new Dictionary<string, Type>()
        {
            { "PREFAB", typeof(GameObject) },
            { "MATERIAL", typeof(Material) },
            { "TEXTURE", typeof(Texture) },
            { "SHADER", typeof(Shader) },
        };

    }


}
