using System;
using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 强制在 Inspector 中显示非序列化字段或属性（即使未标记 [SerializeField]）。
    /// 仅在 HNSmartInspector 中生效。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ShowInInspectorAttribute : Attribute
    {
    }
}
