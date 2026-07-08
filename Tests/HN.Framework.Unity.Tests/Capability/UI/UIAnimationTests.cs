#nullable enable

using System;
using HN.Framework.Unity.Capability.UI;
using LitMotion;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.UI
{
    /// <summary>
    /// UIAnimation 静态工具类的 EditMode 单元测试。
    /// 验证 Fade/Slide/Scale 动画预设的正确性。
    /// </summary>
    [TestFixture]
    public class UIAnimationTests
    {
        private GameObject? _go;
        private CanvasGroup? _canvasGroup;
        private RectTransform? _rectTransform;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestAnimationTarget", typeof(RectTransform));
            _go.hideFlags = HideFlags.HideAndDontSave;

            _canvasGroup = _go.AddComponent<CanvasGroup>();
            _rectTransform = _go.GetComponent<RectTransform>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.DestroyImmediate(_go);
                _go = null;
            }

            _canvasGroup = null;
            _rectTransform = null;
        }

        // ── Fade 测试 ──

        /// <summary>
        /// FadeIn 动画 Complete 后，CanvasGroup.alpha 应为 1。
        /// </summary>
        [Test]
        public void FadeIn_Complete_SetsAlphaToOne()
        {
            _canvasGroup!.alpha = 0f;

            var handle = UIAnimation.FadeIn(_canvasGroup);
            handle.Complete();

            Assert.That(_canvasGroup.alpha, Is.EqualTo(1f));
        }

        /// <summary>
        /// FadeOut 动画 Complete 后，CanvasGroup.alpha 应为 0。
        /// </summary>
        [Test]
        public void FadeOut_Complete_SetsAlphaToZero()
        {
            _canvasGroup!.alpha = 1f;

            var handle = UIAnimation.FadeOut(_canvasGroup);
            handle.Complete();

            Assert.That(_canvasGroup.alpha, Is.EqualTo(0f));
        }

        // ── Scale 测试 ──

        /// <summary>
        /// ScaleIn 动画 Complete 后，localScale 应为 (1,1,1)。
        /// </summary>
        [Test]
        public void ScaleIn_Complete_SetsScaleToOne()
        {
            _rectTransform!.localScale = Vector3.zero;

            var handle = UIAnimation.ScaleIn(_rectTransform);
            handle.Complete();

            Assert.That(_rectTransform.localScale, Is.EqualTo(Vector3.one));
        }

        /// <summary>
        /// ScaleOut 动画 Complete 后，localScale 应为 (0,0,0)。
        /// </summary>
        [Test]
        public void ScaleOut_Complete_SetsScaleToZero()
        {
            _rectTransform!.localScale = Vector3.one;

            var handle = UIAnimation.ScaleOut(_rectTransform);
            handle.Complete();

            Assert.That(_rectTransform.localScale, Is.EqualTo(Vector3.zero));
        }

        // ── Null 参数校验 ──

        /// <summary>
        /// FadeIn 传入 null 目标应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void FadeIn_NullTarget_ThrowsArgumentNullException()
        {
            Assert.That(
                () => UIAnimation.FadeIn(null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("target")
            );
        }

        /// <summary>
        /// ScaleIn 传入 null 目标应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void ScaleIn_NullTarget_ThrowsArgumentNullException()
        {
            Assert.That(
                () => UIAnimation.ScaleIn(null!),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("target")
            );
        }

        /// <summary>
        /// SlideIn 传入 null 目标应抛出 ArgumentNullException。
        /// </summary>
        [Test]
        public void SlideIn_NullTarget_ThrowsArgumentNullException()
        {
            Assert.That(
                () => UIAnimation.SlideIn(null!, SlideDirection.Left),
                Throws.ArgumentNullException.With.Property("ParamName").EqualTo("target")
            );
        }

        // ── 回调测试 ──

        /// <summary>
        /// FadeIn Complete 后 onComplete 回调应触发。
        /// </summary>
        [Test]
        public void FadeIn_OnCompleteCallback_IsCalled()
        {
            _canvasGroup!.alpha = 0f;
            var callbackCalled = false;

            var handle = UIAnimation.FadeIn(_canvasGroup, onComplete: () => callbackCalled = true);
            handle.Complete();

            Assert.That(callbackCalled, Is.True, "onComplete callback should be called after Complete().");
        }
    }
}
