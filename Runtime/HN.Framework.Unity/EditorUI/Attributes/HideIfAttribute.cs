using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 当指定条件字段为 true 时隐藏此字段。
    /// </summary>
    public class HideIfAttribute : PropertyAttribute
    {
        public string Condition { get; }

        public HideIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
