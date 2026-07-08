#nullable enable

using HN.Framework.Core.Capability.UI;
using UnityEngine;

namespace HN.Framework.Unity.Capability.UI
{
    /// <summary>
    /// UI 面板抽象基类，提供生命周期状态机和基础组件引用。
    /// 子类通过重写虚方法（OnOpen、OnClose 等）实现具体的面板行为。
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        private CanvasGroup? _canvasGroup;
        private RectTransform? _rectTransform;

        /// <summary>
        /// 当前面板生命周期状态。
        /// </summary>
        public UIPanelState CurrentState { get; private set; }

        /// <summary>
        /// CanvasGroup 组件，用于透明度（fade）动画控制。
        /// 首次访问时惰性初始化，Open() 中确保组件存在。
        /// </summary>
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup!;
            }
        }

        /// <summary>
        /// RectTransform 组件，用于位置/缩放动画控制。
        /// 首次访问时惰性初始化。
        /// </summary>
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform!;
            }
        }

        /// <summary>
        /// 由 UIManager 调用，打开面板并触发 OnOpen 生命周期回调。
        /// </summary>
        /// <param name="parent">面板挂载到的父 Transform。</param>
        internal void Open(Transform parent)
        {
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            gameObject.SetActive(true);

            // 确保 CanvasGroup 组件存在（惰性初始化）
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            CurrentState = UIPanelState.Opening;
            OnOpen();
            CurrentState = UIPanelState.Opened;
        }

        /// <summary>
        /// 由 UIManager 调用，关闭面板并触发 OnClose 生命周期回调。
        /// </summary>
        internal void Close()
        {
            CurrentState = UIPanelState.Closing;
            OnClose();
            gameObject.SetActive(false);
            CurrentState = UIPanelState.Closed;
        }

        /// <summary>
        /// 面板打开时调用。子类重写以实现入场逻辑（数据绑定、事件注册等）。
        /// </summary>
        protected virtual void OnOpen() { }

        /// <summary>
        /// 面板关闭时调用。子类重写以实现退场逻辑（事件注销、资源释放等）。
        /// </summary>
        protected virtual void OnClose() { }

        /// <summary>
        /// 面板暂停时调用（预留，Phase 2 实现）。
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// 面板恢复时调用（预留，Phase 2 实现）。
        /// </summary>
        protected virtual void OnResume() { }
    }
}
