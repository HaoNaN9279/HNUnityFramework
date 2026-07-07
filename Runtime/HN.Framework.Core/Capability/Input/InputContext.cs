#nullable enable

using System;

namespace HN.Framework.Core.Capability.Input
{
    /// <summary>
    /// 输入上下文，描述一次输入事件的完整信息。
    /// </summary>
    public readonly struct InputContext : IEquatable<InputContext>
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public readonly string? ActionName;

        /// <summary>
        /// 输入阶段
        /// </summary>
        public readonly InputPhase Phase;

        /// <summary>
        /// 输入值
        /// </summary>
        public readonly object? Value;

        /// <summary>
        /// 初始化输入上下文
        /// </summary>
        /// <param name="actionName">动作名称</param>
        /// <param name="phase">输入阶段</param>
        /// <param name="value">输入值</param>
        public InputContext(string? actionName, InputPhase phase, object? value)
        {
            ActionName = actionName;
            Phase = phase;
            Value = value;
        }

        /// <summary>
        /// 返回输入上下文的调试字符串
        /// </summary>
        public override string ToString()
        {
            return $"[InputContext] ActionName={ActionName}, Phase={Phase}, Value={Value}";
        }

        /// <summary>
        /// 判断两个输入上下文是否相等
        /// </summary>
        public bool Equals(InputContext other)
        {
            return ActionName == other.ActionName && Phase == other.Phase && Equals(Value, other.Value);
        }

        /// <summary>
        /// 判断当前实例是否等于指定对象
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is InputContext other && Equals(other);
        }

        /// <summary>
        /// 获取哈希值
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ActionName?.GetHashCode() ?? 0);
                hash = hash * 23 + Phase.GetHashCode();
                hash = hash * 23 + (Value?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// 判断两个输入上下文是否相等
        /// </summary>
        public static bool operator ==(InputContext left, InputContext right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 判断两个输入上下文是否不相等
        /// </summary>
        public static bool operator !=(InputContext left, InputContext right)
        {
            return !left.Equals(right);
        }
    }
}
