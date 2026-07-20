using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 当指定条件字段为 true 时显示此字段。
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        public string Condition { get; }

        public ShowIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
