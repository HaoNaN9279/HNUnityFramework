using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace HN.Framework.Editor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class SheetFieldTypeAttribute : Attribute
    {
        public SheetFieldTypeAttribute(string typeName)
        {
            this.typeName = typeName;
        }


        public string typeName;
    }
}
