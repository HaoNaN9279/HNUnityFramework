using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 标记字段为必填。当字段值为 null 时，在 Inspector 中显示红色警告。
    /// </summary>
    public class RequiredAttribute : PropertyAttribute
    {
        /// <summary>自定义错误消息，为空时使用默认提示</summary>
        public string ErrorMessage { get; set; }

        public RequiredAttribute()
        {
        }

        /// <param name="errorMessage">自定义错误消息</param>
        public RequiredAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
