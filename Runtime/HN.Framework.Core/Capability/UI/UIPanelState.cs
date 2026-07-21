#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// UI 面板生命周期状态枚举。
    /// </summary>
    public enum UIPanelState
    {
        /// <summary>
        /// 面板已关闭，不在场景中。
        /// </summary>
        Closed,

        /// <summary>
        /// 面板正在打开（播放入场动画）。
        /// </summary>
        Opening,

        /// <summary>
        /// 面板已完全打开并可见。
        /// </summary>
        Opened,

        /// <summary>
        /// 面板正在关闭（播放退场动画）。
        /// </summary>
        Closing
    }
}
