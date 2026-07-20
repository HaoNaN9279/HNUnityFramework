using System;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 强制在 HNSmartInspector 中隐藏字段（即使标记了 [SerializeField]）。
    /// 由框架内部使用，在字段被 [ShowInInspector] 显示但需要条件隐藏时使用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class HideInInspectorAttribute : Attribute
    {
    }
}
