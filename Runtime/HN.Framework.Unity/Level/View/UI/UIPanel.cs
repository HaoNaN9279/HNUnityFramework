#nullable enable

using HN.Framework.Core.Capability.UI;
using LitMotion;
using UnityEngine;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// UI 面板抽象基类，提供生命周期状态机和基础组件引用。
    /// 子类通过重写虚方法（OnOpen、OnClose 等）实现具体的面板行为。
    /// Phase 2 增加了动画支持（ONEnterAnimation/OnExitAnimation）。
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        private CanvasGroup? _canvasGroup;
        private RectTransform? _rectTransform;

        /// <summary>
        /// 入场动画句柄，子类可用于控制入场动画的 Complete/Cancel。
        /// </summary>
        protected MotionHandle? _enterHandle;

        /// <summary>
        /// 退场动画句柄，子类可用于控制退场动画的 Complete/Cancel。
        /// </summary>
        protected MotionHandle? _exitHandle;

        /// <summary>
        /// 入场动画持续时间（秒）。子类重写以启用入场淡入动画，返回 0 表示禁用。
        /// </summary>
        protected virtual float OnEnterAnimationDuration => 0f;

        /// <summary>
        /// 退场动画持续时间（秒）。子类重写以启用退场淡出动画，返回 0 表示禁用。
        /// </summary>
        protected virtual float OnExitAnimationDuration => 0f;

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

            // 播放入场动画
            if (OnEnterAnimationDuration > 0f && CanvasGroup != null)
            {
                // 先取消之前的入场动画（防止重复 Open 导致冲突）
                if (_enterHandle.HasValue && _enterHandle.Value.IsActive())
                {
                    _enterHandle.Value.Cancel();
                }
                _enterHandle = UIAnimation.FadeIn(CanvasGroup, OnEnterAnimationDuration);
            }
        }

        /// <summary>
        /// 由 UIManager 调用，关闭面板并触发 OnClose 生命周期回调。
        /// </summary>
        internal void Close()
        {
            // 在使用任何组件前检查 GameObject 是否仍然有效
            if (gameObject == null)
                return;

            // 取消可能还在运行的入场动画
            if (_enterHandle.HasValue && _enterHandle.Value.IsActive())
            {
                _enterHandle.Value.Cancel();
            }

            // 播放并完成退场动画
            if (OnExitAnimationDuration > 0f && _canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _exitHandle = UIAnimation.FadeOut(_canvasGroup, OnExitAnimationDuration);
                // 直接设置最终 alpha 值并取消动画，避免 LitMotion 在 GameObject 销毁后尝试访问 CanvasGroup
                _canvasGroup.alpha = 0f;
                _exitHandle.Value.Cancel();
            }

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
        /// 面板暂停时调用（当新面板被 Push 到导航栈时）。
        /// </summary>
        protected virtual void OnPause() { }

        /// <summary>
        /// 面板恢复时调用（当栈顶面板被 Pop 后，下一层面板回到前台）。
        /// </summary>
        protected virtual void OnResume() { }

        /// <summary>
        /// 取消所有正在进行的面板动画（入场和退场）。
        /// </summary>
        public void CancelAnimations()
        {
            if (_enterHandle.HasValue && _enterHandle.Value.IsActive())
                _enterHandle.Value.Cancel();
            if (_exitHandle.HasValue && _exitHandle.Value.IsActive())
                _exitHandle.Value.Cancel();
        }

        /// <summary>
        /// 内部方法：暂停面板。由 UIManager 在 Push 新面板时对栈顶面板调用。
        /// </summary>
        internal void Pause()
        {
            OnPause();
        }

        /// <summary>
        /// 内部方法：恢复面板。由 UIManager 在 Pop 后对新的栈顶面板调用。
        /// </summary>
        internal void Resume()
        {
            OnResume();
        }
    }
}
