#nullable enable

using System;
using UnityEngine;
using UnityEngine.UI;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// 模态弹窗基类，继承 UIPanel，提供确认/取消回调支持和模态遮罩。
    /// 弹出时阻挡下层 UI 交互，关闭后恢复。
    /// </summary>
    public abstract class UIDialog : UIPanel
    {
        /// <summary>
        /// 确认按钮引用，子类在 Inspector 中绑定。
        /// </summary>
        [SerializeField]
        private Button? confirmButton;

        /// <summary>
        /// 取消按钮引用，子类在 Inspector 中绑定。
        /// </summary>
        [SerializeField]
        private Button? cancelButton;

        /// <summary>
        /// 模态遮罩 Image 引用，用于绘制半透明背景阻挡下层交互。
        /// </summary>
        [SerializeField]
        private Image? maskImage;

        /// <summary>
        /// 确认按钮点击后执行的回调。
        /// </summary>
        public Action? OnConfirm { get; set; }

        /// <summary>
        /// 取消按钮点击后执行的回调。
        /// </summary>
        public Action? OnCancel { get; set; }

        /// <summary>
        /// 显示模态弹窗，设置父节点、回调，并确保 CanvasGroup 允许射线检测后打开面板。
        /// </summary>
        /// <param name="parent">弹窗挂载到的父 Transform。</param>
        /// <param name="onConfirm">确认回调。</param>
        /// <param name="onCancel">取消回调。</param>
        internal void Show(Transform parent, Action? onConfirm, Action? onCancel)
        {
            OnConfirm = onConfirm;
            OnCancel = onCancel;

            base.Open(parent);

            CanvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 面板打开时注册按钮监听并设置遮罩透明度。
        /// </summary>
        protected override void OnOpen()
        {
            base.OnOpen();

            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            if (maskImage != null)
            {
                var color = maskImage.color;
                color.a = UIManager.DialogMaskAlpha;
                maskImage.color = color;
            }
        }

        /// <summary>
        /// 面板关闭时重置射线检测并注销按钮监听。
        /// </summary>
        protected override void OnClose()
        {
            base.OnClose();

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.blocksRaycasts = false;

            if (confirmButton != null)
                confirmButton.onClick.RemoveListener(OnConfirmClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        /// <summary>
        /// 确认按钮点击处理，触发 OnConfirm 回调后关闭面板。
        /// 子类可重写以添加自定义逻辑（如数据校验）。
        /// </summary>
        protected virtual void OnConfirmClicked()
        {
            OnConfirm?.Invoke();
            Close();
        }

        /// <summary>
        /// 取消按钮点击处理，触发 OnCancel 回调后关闭面板。
        /// 子类可重写以添加自定义逻辑（如确认放弃未保存更改）。
        /// </summary>
        protected virtual void OnCancelClicked()
        {
            OnCancel?.Invoke();
            Close();
        }
    }
}
