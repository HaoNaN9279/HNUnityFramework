using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 当指定条件字段为 true 时启用该字段（可编辑），否则禁用（灰色不可编辑）。
    /// 条件字段必须是同脚本中的 bool 类型字段（serialized 或非 serialized 均可）。
    /// </summary>
    public class EnableIfAttribute : PropertyAttribute
    {
        public string Condition { get; }

        /// <param name="condition">条件字段名（bool 类型）</param>
        public EnableIfAttribute(string condition)
        {
            Condition = condition;
        }
    }
}
