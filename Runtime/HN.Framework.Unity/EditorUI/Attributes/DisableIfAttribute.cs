using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 当指定条件字段为 true 时禁用该字段（灰色不可编辑）。
    /// 与 EnableIf 逻辑相反。
    /// </summary>
    public class DisableIfAttribute : PropertyAttribute
    {
        public string Condition { get; }

        /// <param name="condition">条件字段名（bool 类型）</param>
        public DisableIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
