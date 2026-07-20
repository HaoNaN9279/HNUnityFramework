using UnityEngine;

namespace HN.Framework.Unity.EditorUI.Attributes
{
    /// <summary>
    /// 当字段值发生变化时，调用指定的方法。
    /// 方法签名：void MethodName() 或 void MethodName(object newValue)
    /// </summary>
    public class OnValueChangedAttribute : PropertyAttribute
    {
        public string MethodName { get; }

        /// <param name="methodName">值变化时调用的方法名</param>
        public OnValueChangedAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
