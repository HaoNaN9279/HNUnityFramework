#nullable enable

using System;
using LitMotion;
using UnityEngine;

namespace HN.Framework.Unity.Level.View.UI
{
    /// <summary>
    /// 滑动方向枚举，用于 SlideIn/SlideOut 动画。
    /// </summary>
    public enum SlideDirection
    {
        /// <summary>从左侧滑入 / 向左侧滑出。</summary>
        Left,

        /// <summary>从右侧滑入 / 向右侧滑出。</summary>
        Right,

        /// <summary>从顶部滑入 / 向顶部滑出。</summary>
        Top,

        /// <summary>从底部滑入 / 向底部滑出。</summary>
        Bottom
    }

    /// <summary>
    /// UI 动画预设静态工具类，基于 LitMotion 提供淡入淡出、滑动、缩放等常用 UI 动画。
    /// 所有方法返回 <see cref="MotionHandle"/>，可用于手动 Complete、Cancel 等控制。
    /// </summary>
    public static class UIAnimation
    {
        // ── Fade ──

        /// <summary>
        /// 淡入动画：将 <see cref="CanvasGroup.alpha"/> 从 0 过渡到 1。
        /// </summary>
        /// <param name="target">目标 CanvasGroup，不可为 null。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle FadeIn(CanvasGroup target, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var builder = LMotion.Create(0f, 1f, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(x => target.alpha = x);
        }

        /// <summary>
        /// 淡出动画：将 <see cref="CanvasGroup.alpha"/> 从 1 过渡到 0。
        /// </summary>
        /// <param name="target">目标 CanvasGroup，不可为 null。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle FadeOut(CanvasGroup target, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var builder = LMotion.Create(1f, 0f, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(x => target.alpha = x);
        }

        // ── Slide ──

        /// <summary>
        /// 滑入动画：将 <see cref="RectTransform.anchoredPosition"/> 从屏幕外侧过渡到 (0, 0)。
        /// </summary>
        /// <param name="target">目标 RectTransform，不可为 null。</param>
        /// <param name="from">滑入起始方向。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle SlideIn(RectTransform target, SlideDirection from, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var startPos = GetSlideOffset(from);
            var builder = LMotion.Create(startPos, Vector2.zero, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(v => target.anchoredPosition = v);
        }

        /// <summary>
        /// 滑出动画：将 <see cref="RectTransform.anchoredPosition"/> 从 (0, 0) 过渡到屏幕外侧。
        /// </summary>
        /// <param name="target">目标 RectTransform，不可为 null。</param>
        /// <param name="to">滑出目标方向。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle SlideOut(RectTransform target, SlideDirection to, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var endPos = GetSlideOffset(to);
            var builder = LMotion.Create(Vector2.zero, endPos, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(v => target.anchoredPosition = v);
        }

        // ── Scale ──

        /// <summary>
        /// 缩放入动画：将 <see cref="Transform.localScale"/> 从 (0, 0, 0) 过渡到 (1, 1, 1)。
        /// </summary>
        /// <param name="target">目标 RectTransform，不可为 null。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle ScaleIn(RectTransform target, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var builder = LMotion.Create(Vector3.zero, Vector3.one, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(v => target.localScale = v);
        }

        /// <summary>
        /// 缩放退动画：将 <see cref="Transform.localScale"/> 从 (1, 1, 1) 过渡到 (0, 0, 0)。
        /// </summary>
        /// <param name="target">目标 RectTransform，不可为 null。</param>
        /// <param name="duration">动画持续时间（秒），默认 0.3。</param>
        /// <param name="ease">缓动函数，默认 <see cref="Ease.InOutQuad"/>。</param>
        /// <param name="onComplete">动画完成时触发的回调。</param>
        /// <returns>MotionHandle，可用于控制动画（Complete、Cancel 等）。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="target"/> 为 null 时抛出。</exception>
        public static MotionHandle ScaleOut(RectTransform target, float duration = 0.3f, Ease? ease = null, Action? onComplete = null)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            var builder = LMotion.Create(Vector3.one, Vector3.zero, duration)
                .WithEase(ease ?? Ease.InOutQuad);
            if (onComplete != null)
                builder = builder.WithOnComplete(onComplete);
            return builder.Bind(v => target.localScale = v);
        }

        // ── Helpers ──

        /// <summary>
        /// 根据滑动方向计算屏幕外侧的起始/结束偏移量。
        /// </summary>
        /// <param name="direction">滑动方向。</param>
        /// <returns>anchoredPosition 偏移量。</returns>
        private static Vector2 GetSlideOffset(SlideDirection direction)
        {
            return direction switch
            {
                SlideDirection.Left => new Vector2(-Screen.width, 0f),
                SlideDirection.Right => new Vector2(Screen.width, 0f),
                SlideDirection.Top => new Vector2(0f, Screen.height),
                SlideDirection.Bottom => new Vector2(0f, -Screen.height),
                _ => Vector2.zero
            };
        }
    }
}
