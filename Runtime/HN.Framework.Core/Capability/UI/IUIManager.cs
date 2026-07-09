#nullable enable

using System;

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
        [Obsolete("Use PushAsync for Addressables loading.")]
        void Push(string panelPath);

        /// <summary>
        /// 弹出导航栈顶面板并返回上一级。
        /// </summary>
        [Obsolete("Use Pop() is still valid for stack navigation.")]
        void Pop();

        /// <summary>
        /// 以模态方式显示面板（不在导航栈中）。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        [Obsolete("Use ShowAsync for Addressables loading.")]
        void Show(string panelPath);

        /// <summary>
        /// 隐藏指定的模态面板。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        [Obsolete("Use Hide() is still valid.")]
        void Hide(string panelPath);

        // ───────── 新增方法 ─────────

        /// <summary>
        /// 显示对话框面板，支持确认和取消回调。
        /// </summary>
        /// <param name="panelPath">对话框面板资源路径。</param>
        /// <param name="onConfirm">用户点击确认时的回调。</param>
        /// <param name="onCancel">用户点击取消时的回调。</param>
        void ShowDialog(string panelPath, Action? onConfirm = null, Action? onCancel = null);

        /// <summary>
        /// 显示浮动提示消息（Toast）。
        /// </summary>
        /// <param name="message">提示文本内容。</param>
        /// <param name="duration">显示持续时间（秒），默认 2 秒。</param>
        void ShowToast(string message, float duration = 2f);

        /// <summary>
        /// 启动指定标识的新手引导流程。
        /// </summary>
        /// <param name="guideId">引导流程的唯一标识。</param>
        /// <param name="onCompleted">引导完成时的回调。</param>
        void StartGuide(string guideId, Action? onCompleted = null);

        /// <summary>
        /// 停止当前正在运行的新手引导流程。
        /// </summary>
        void StopGuide();

        /// <summary>
        /// 异步加载面板资源后推入导航栈并显示。
        /// 与 <see cref="Push"/> 不同，此方法会先通过 Addressables 加载资源再显示。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        void PushAsync(string panelPath);

        /// <summary>
        /// 异步加载面板资源后以模态方式显示。
        /// 与 <see cref="Show"/> 不同，此方法会先通过 Addressables 加载资源再显示。
        /// </summary>
        /// <param name="panelPath">面板资源路径。</param>
        void ShowAsync(string panelPath);
    }
}
