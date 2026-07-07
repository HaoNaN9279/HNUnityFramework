using System;

namespace HN.Framework.Core.Capability.Input
{
    /// <summary>
    /// 输入管理器接口，负责输入动作的注册/注销、ActionMap 管理及控制方案切换。
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// 注册输入动作回调。
        /// </summary>
        /// <param name="actionName">输入动作名称。</param>
        /// <param name="callback">输入上下文回调。</param>
        void RegisterAction(string actionName, Action<InputContext> callback);

        /// <summary>
        /// 注销输入动作回调。
        /// </summary>
        /// <param name="actionName">输入动作名称。</param>
        /// <param name="callback">输入上下文回调。</param>
        void UnregisterAction(string actionName, Action<InputContext> callback);

        /// <summary>
        /// 启用指定的 ActionMap。
        /// </summary>
        /// <param name="mapName">ActionMap 名称。</param>
        void EnableActionMap(string mapName);

        /// <summary>
        /// 禁用指定的 ActionMap。
        /// </summary>
        /// <param name="mapName">ActionMap 名称。</param>
        void DisableActionMap(string mapName);

        /// <summary>
        /// 切换控制方案。
        /// </summary>
        /// <param name="scheme">控制方案名称（如 "KeyboardMouse"、"Gamepad"、"Touch"）。</param>
        void SetControlScheme(string scheme);

        /// <summary>
        /// 获取输入屏蔽器实例。
        /// </summary>
        IInputBlocker Blocker { get; }
    }
}
