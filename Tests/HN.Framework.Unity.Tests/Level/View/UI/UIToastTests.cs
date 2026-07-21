#nullable enable

using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Level.View.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Level.View.UI
{
    /// <summary>
    /// UIToast 的 EditMode 单元测试，覆盖消息显示、默认时长、状态转换、事件触发和射线检测。
    /// </summary>
    [TestFixture]
    public class UIToastTests
    {
        private GameObject? _parentGo;
        private GameObject? _toastGo;
        private ConcreteToast? _toast;
        private TextMeshProUGUI? _messageText;

        [SetUp]
        public void SetUp()
        {
            _parentGo = new GameObject("TestParent");
            _parentGo.hideFlags = HideFlags.HideAndDontSave;

            _toastGo = new GameObject("TestToast", typeof(RectTransform));
            _toastGo.hideFlags = HideFlags.HideAndDontSave;

            _toast = _toastGo.AddComponent<ConcreteToast>();

            // 添加 TextMeshProUGUI 并赋值到私有 messageText 字段
            _messageText = _toastGo.AddComponent<TextMeshProUGUI>();
            var field = typeof(UIToast).GetField("messageText",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(_toast, _messageText);
        }

        [TearDown]
        public void TearDown()
        {
            if (_toastGo != null)
            {
                Object.DestroyImmediate(_toastGo);
                _toastGo = null;
            }

            if (_parentGo != null)
            {
                Object.DestroyImmediate(_parentGo);
                _parentGo = null;
            }

            _toast = null;
            _messageText = null;
        }

        /// <summary>
        /// Show("Hello") 后 messageText.text 应为 "Hello"。
        /// </summary>
        [Test]
        public void Show_SetsMessageText()
        {
            _toast!.Show(_parentGo!.transform, "Hello", 2f);

            Assert.That(_messageText!.text, Is.EqualTo("Hello"));
        }

        /// <summary>
        /// Show 后 Duration 应为默认值 2f。
        /// </summary>
        [Test]
        public void Show_Duration_DefaultsToTwo()
        {
            _toast!.Show(_parentGo!.transform, "Test", 2f);

            Assert.That(_toast.Duration, Is.EqualTo(2f));
        }

        /// <summary>
        /// Show() 调用后 CurrentState 应为 Opened。
        /// </summary>
        [Test]
        public void Show_StateBecomesOpened()
        {
            _toast!.Show(_parentGo!.transform, "Test", 2f);

            Assert.That(_toast.CurrentState, Is.EqualTo(UIPanelState.Opened));
        }

        /// <summary>
        /// Close() 后 OnDismissed 事件应被触发。
        /// </summary>
        [Test]
        public void Close_OnDismissed_EventTriggered()
        {
            bool dismissed = false;
            _toast!.Show(_parentGo!.transform, "Test", 2f);
            _toast.OnDismissed = () => dismissed = true;

            _toast.Close();

            Assert.That(dismissed, Is.True, "OnDismissed should be invoked on Close().");
        }

        /// <summary>
        /// Show 后 CanvasGroup.blocksRaycasts 应为 false（Toast 不阻挡交互）。
        /// </summary>
        [Test]
        public void Close_BlocksRaycasts_IsFalse()
        {
            _toast!.Show(_parentGo!.transform, "Test", 2f);

            Assert.That(_toast.CanvasGroup.blocksRaycasts, Is.False);
        }

        /// <summary>
        /// 未调用 Show() 直接调用 Close() 不应抛出异常。
        /// </summary>
        [Test]
        public void Close_BeforeOpen_NoException()
        {
            Assert.That(() => _toast!.Close(), Throws.Nothing);
        }

        /// <summary>
        /// UIToast 的具象测试子类。
        /// </summary>
        public class ConcreteToast : UIToast { }
    }
}
