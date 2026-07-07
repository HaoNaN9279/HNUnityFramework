#nullable enable

namespace HN.Framework.Core.Capability.Input
{
    /// <summary>
    /// 输入动作类型
    /// </summary>
    public enum InputActionType
    {
        /// <summary>按钮（瞬时）</summary>
        Button,
        /// <summary>数值（一维）</summary>
        Value,
        /// <summary>二维向量</summary>
        Vector2,
    }

    /// <summary>
    /// 输入动作阶段
    /// </summary>
    public enum InputPhase
    {
        /// <summary>开始</summary>
        Started,
        /// <summary>持续</summary>
        Performed,
        /// <summary>取消</summary>
        Canceled,
    }
}
