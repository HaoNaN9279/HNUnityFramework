using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 使用指定的校验方法验证字段值。
    /// 校验方法签名：bool MethodName(object value, out string errorMessage)
    /// </summary>
    public class ValidateInputAttribute : PropertyAttribute
    {
        /// <summary>校验方法名</summary>
        public string MethodName { get; }
        /// <summary>校验失败时显示的默认消息</summary>
        public string Message { get; set; }

        /// <param name="methodName">校验方法名，签名：bool MethodName(object value, out string message)</param>
        public ValidateInputAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
