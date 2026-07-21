#nullable enable

using HN.Framework.Core.Driver.Common;

namespace HN.Framework.Core.Capability.Input
{
    /// <summary>
    /// 输入动作，表示一个可通过引用池复用的输入操作定义。
    /// </summary>
    public sealed class InputAction : IReference
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        public InputActionType ActionType { get; set; }

        /// <summary>
        /// 默认绑定字符串
        /// </summary>
        public string? DefaultBinding { get; set; }

        /// <summary>
        /// 清理对象状态，将全部属性重置为默认值。
        /// </summary>
        public void Clear()
        {
            ActionName = null;
            ActionType = default;
            DefaultBinding = null;
        }
    }
}
