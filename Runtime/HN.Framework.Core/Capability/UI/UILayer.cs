#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// UI 层级枚举，定义面板的渲染排序层级。
    /// 值越大排序越靠前（显示在上层）。
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层（SortOrder: 0），用于背景图片等最底层元素。
        /// </summary>
        Background = 0,

        /// <summary>
        /// 场景层（SortOrder: 100），用于 3D/2D 场景内嵌 UI。
        /// </summary>
        Scene = 100,

        /// <summary>
        /// 主界面层（SortOrder: 200），用于常驻 HUD、主菜单等。
        /// </summary>
        UI = 200,

        /// <summary>
        /// 弹窗层（SortOrder: 300），用于对话框、确认框等模态弹窗。
        /// </summary>
        Popup = 300,

        /// <summary>
        /// 提示层（SortOrder: 400），用于浮动提示、Toast 等短暂显示的信息。
        /// </summary>
        Toast = 400,

        /// <summary>
        /// 引导层（SortOrder: 500），用于新手引导、教程遮罩等。
        /// </summary>
        Guide = 500,

        /// <summary>
        /// 系统层（SortOrder: 600），用于系统级弹窗、Loading 界面等最顶层元素。
        /// </summary>
        System = 600
    }
}
