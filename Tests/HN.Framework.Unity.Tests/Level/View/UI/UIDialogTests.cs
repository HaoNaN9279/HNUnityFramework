#nullable enable

using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Level.View.UI;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.View.UI
{
    /// <summary>
    /// UIDialog 基类的 EditMode 单元测试，覆盖模态弹窗的确认/取消回调和射线检测状态。
    /// </summary>
    [TestFixture]
    public class UIDialogTests
    {
        private GameObject? _parentGo;
        private GameObject? _dialogGo;
        private ConcreteDialog? _dialog;

        [SetUp]
        public void SetUp()
        {
            _parentGo = new GameObject("TestParent");
            _parentGo.hideFlags = HideFlags.HideAndDontSave;

            _dialogGo = new GameObject("TestDialog", typeof(RectTransform));
            _dialogGo.hideFlags = HideFlags.HideAndDontSave;

            _dialog = _dialogGo.AddComponent<ConcreteDialog>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_dialogGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_dialogGo);
                _dialogGo = null;
            }

            if (_parentGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_parentGo);
                _parentGo = null;
            }

            _dialog = null;
        }

        // ── 生命周期与模态 ──

        /// <summary>
        /// Show() 调用后 CurrentState 应为 Opened。
        /// </summary>
        [Test]
        public void Show_ParentSet_StateBecomesOpened()
        {
            _dialog!.Show(_parentGo!.transform, null, null);

            Assert.That(_dialog.CurrentState, Is.EqualTo(UIPanelState.Opened));
        }

        /// <summary>
        /// Show() 调用后 CanvasGroup.blocksRaycasts 应为 true，阻挡下层交互。
        /// </summary>
        [Test]
        public void Show_BlocksRaycasts_IsTrue()
        {
            _dialog!.Show(_parentGo!.transform, null, null);

            Assert.That(_dialog.CanvasGroup.blocksRaycasts, Is.True);
        }

        // ── 确认/取消回调 ──

        /// <summary>
        /// OnConfirmClicked() 应触发 OnConfirm 回调。
        /// </summary>
        [Test]
        public void Confirm_CallsOnConfirmCallback()
        {
            bool callbackCalled = false;
            _dialog!.OnConfirm = () => callbackCalled = true;

            _dialog.InvokeConfirmClicked();

            Assert.That(callbackCalled, Is.True, "OnConfirm callback should be invoked.");
        }

        /// <summary>
        /// OnCancelClicked() 应触发 OnCancel 回调。
        /// </summary>
        [Test]
        public void Cancel_CallsOnCancelCallback()
        {
            bool callbackCalled = false;
            _dialog!.OnCancel = () => callbackCalled = true;

            _dialog.InvokeCancelClicked();

            Assert.That(callbackCalled, Is.True, "OnCancel callback should be invoked.");
        }

        // ── 关闭后射线检测重置 ──

        /// <summary>
        /// Close() 调用后 blocksRaycasts 应恢复为 false。
        /// </summary>
        [Test]
        public void Close_BlocksRaycasts_ResetsToFalse()
        {
            _dialog!.Show(_parentGo!.transform, null, null);
            _dialog.Close();

            Assert.That(_dialog.CanvasGroup.blocksRaycasts, Is.False);
        }

        // ── 空回调安全 ──

        /// <summary>
        /// OnConfirmCallback 为 null 时调用 OnConfirmClicked() 不应抛出异常。
        /// </summary>
        [Test]
        public void Confirm_Callback_IsNullSafe()
        {
            _dialog!.OnConfirm = null;

            Assert.That(() => _dialog.InvokeConfirmClicked(), Throws.Nothing);
        }

        /// <summary>
        /// OnCancelCallback 为 null 时调用 OnCancelClicked() 不应抛出异常。
        /// </summary>
        [Test]
        public void Cancel_Callback_IsNullSafe()
        {
            _dialog!.OnCancel = null;

            Assert.That(() => _dialog.InvokeCancelClicked(), Throws.Nothing);
        }

        // ── 测试用具象子类 ──

        /// <summary>
        /// UIDialog 的具象测试子类，暴露 protected 方法供测试调用。
        /// </summary>
        public class ConcreteDialog : UIDialog
        {
            /// <summary>
            /// 公开调用 OnConfirmClicked，供测试使用。
            /// </summary>
            public void InvokeConfirmClicked() => OnConfirmClicked();

            /// <summary>
            /// 公开调用 OnCancelClicked，供测试使用。
            /// </summary>
            public void InvokeCancelClicked() => OnCancelClicked();
        }
    }
}
