#nullable enable

namespace HN.Framework.Core.Capability.UI
{
    /// <summary>
    /// UI 管理器接口，定义栈式导航与模态面板的显示/隐藏操作。
    /// 注意：GetLayerCanvas 方法不在此接口中，因为 Canvas 是 UnityEngine 类型，
    /// 而 Core 程序集开启了 noEngineReferences。
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 将面板推入导航栈并显示。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        void Push(string panelPath);

        /// <summary>
        /// 弹出导航栈顶面板并返回上一级。
        /// </summary>
        void Pop();

        /// <summary>
        /// 以模态方式显示面板（不在导航栈中）。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        void Show(string panelPath);

        /// <summary>
        /// 隐藏指定的模态面板。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        void Hide(string panelPath);
    }
}
