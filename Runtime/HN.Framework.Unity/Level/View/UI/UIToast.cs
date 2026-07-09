#nullable enable

using System;
using TMPro;
using UnityEngine;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// 自动消失提示面板，用于短暂显示通知消息。
    /// 不阻挡下层 UI 交互（blocksRaycasts = false），
    /// 自动消失计时由 UIManager 外部管理。
    /// 动画通过 <see cref="UIPanel.OnEnterAnimationDuration"/> / <see cref="OnExitAnimationDuration"/> 控制。
    /// </summary>
    public class UIToast : UIPanel
    {
        /// <summary>
        /// 消息文本组件引用，由子类在 Inspector 中绑定。
        /// 程序化创建 Toast 时可通过 <see cref="SetMessageTextComponent"/> 设置。
        /// </summary>
        [SerializeField]
        internal TextMeshProUGUI? messageText;

        /// <summary>
        /// 获取或设置消息显示文本。
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置显示时长（秒），默认 2 秒。
        /// 注意：UIToast 本身不管理计时器，由 UIManager 负责。
        /// </summary>
        public float Duration { get; set; } = 2f;

        /// <summary>
        /// Toast 关闭时触发的事件。
        /// </summary>
        public Action? OnDismissed;

        /// <summary>
        /// 入场动画持续时间（秒）。重写以使用淡入效果。
        /// </summary>
        protected override float OnEnterAnimationDuration => 0.2f;

        /// <summary>
        /// 退场动画持续时间（秒）。重写以使用淡出效果。
        /// </summary>
        protected override float OnExitAnimationDuration => 0.15f;

        /// <summary>
        /// 显示 Toast 消息。
        /// </summary>
        /// <param name="parent">挂载到的父 Transform。</param>
        /// <param name="message">要显示的消息文本。</param>
        /// <param name="duration">显示时长（秒）。</param>
        internal void Show(Transform parent, string message, float duration)
        {
            Message = message;
            Duration = duration;
            base.Open(parent);
            CanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 面板打开时调用。设置消息文本并设置初始透明状态。
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();

            if (messageText != null)
                messageText.text = Message;

            CanvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 面板关闭时调用。触发 OnDismissed 事件通知 UIManager。
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();
            OnDismissed?.Invoke();
        }
    }
}
