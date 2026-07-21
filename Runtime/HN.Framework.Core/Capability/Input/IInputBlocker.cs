namespace HN.Framework.Core.Capability.Input
{
    /// <summary>
    /// 输入屏蔽器接口，提供基于优先级的输入屏蔽栈。
    /// 更高优先级的屏蔽会阻挡较低优先级的输入动作。
    /// 相同优先级时，后 Push 的 token 生效。
    /// </summary>
    public interface IInputBlocker
    {
        /// <summary>
        /// 推入一个输入屏蔽令牌。
        /// </summary>
        /// <param name="token">屏蔽令牌对象。</param>
        /// <param name="priority">屏蔽优先级，数值越大优先级越高。</param>
        void Push(object token, int priority);

        /// <summary>
        /// 弹出指定的屏蔽令牌。若令牌不存在则为空操作。
        /// </summary>
        /// <param name="token">屏蔽令牌对象。</param>
        void Pop(object token);

        /// <summary>
        /// 检查指定优先级的输入动作是否被屏蔽。
        /// </summary>
        /// <param name="actionPriority">输入动作的优先级。</param>
        /// <returns>如果被屏蔽则返回 true，否则返回 false。</returns>
        bool IsBlocked(int actionPriority);

        /// <summary>
        /// 清除所有屏蔽令牌。
        /// </summary>
        void Clear();
    }
}
