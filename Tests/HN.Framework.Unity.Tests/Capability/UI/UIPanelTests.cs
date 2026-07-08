#nullable enable

using HN.Framework.Core.Capability.UI;
using HN.Framework.Unity.Capability.UI;
using NUnit.Framework;
using UnityEngine;

namespace HN.Framework.Unity.Tests.Capability.UI
{
    /// <summary>
    /// UIPanel 基类的 EditMode 单元测试，覆盖生命周期状态机与组件初始化。
    /// </summary>
    [TestFixture]
    public class UIPanelTests
    {
        private GameObject? _parentGo;
        private GameObject? _panelGo;
        private ConcretePanel? _panel;

        [SetUp]
        public void SetUp()
        {
            _parentGo = new GameObject("TestParent");
            _parentGo.hideFlags = HideFlags.HideAndDontSave;

            _panelGo = new GameObject("TestPanel", typeof(RectTransform));
            _panelGo.hideFlags = HideFlags.HideAndDontSave;

            _panel = _panelGo.AddComponent<ConcretePanel>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_panelGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_panelGo);
                _panelGo = null;
            }

            if (_parentGo != null)
            {
                UnityEngine.Object.DestroyImmediate(_parentGo);
                _parentGo = null;
            }

            _panel = null;
        }

        // ── 生命周期 ──

        /// <summary>
        /// Open() 调用后 CurrentState 应为 Opened。
        /// </summary>
        [Test]
        public void Open_ParentSet_StateBecomesOpened()
        {
            _panel!.Open(_parentGo!.transform);

            Assert.That(_panel.CurrentState, Is.EqualTo(UIPanelState.Opened));
        }

        /// <summary>
        /// Close() 调用后 CurrentState 应为 Closed。
        /// </summary>
        [Test]
        public void Close_AfterOpen_StateBecomesClosed()
        {
            _panel!.Open(_parentGo!.transform);
            _panel.Close();

            Assert.That(_panel.CurrentState, Is.EqualTo(UIPanelState.Closed));
        }

        /// <summary>
        /// Open() 应调用虚方法 OnOpen()。
        /// </summary>
        [Test]
        public void OnOpen_VirtualMethod_Called()
        {
            _panel!.Open(_parentGo!.transform);

            Assert.That(_panel.OnOpenCalled, Is.True, "OnOpen() should be called during Open().");
        }

        /// <summary>
        /// Close() 应调用虚方法 OnClose()。
        /// </summary>
        [Test]
        public void OnClose_VirtualMethod_Called()
        {
            _panel!.Open(_parentGo!.transform);
            _panel.Close();

            Assert.That(_panel.OnCloseCalled, Is.True, "OnClose() should be called during Close().");
        }

        // ── 健壮性 ──

        /// <summary>
        /// 重复调用 Open() 不应抛出异常。
        /// </summary>
        [Test]
        public void DoubleOpen_NoException()
        {
            _panel!.Open(_parentGo!.transform);

            Assert.That(() => _panel.Open(_parentGo!.transform), Throws.Nothing);
        }

        /// <summary>
        /// 未调用 Open() 直接调用 Close() 不应抛出异常。
        /// </summary>
        [Test]
        public void Close_BeforeOpen_NoException()
        {
            Assert.That(() => _panel!.Close(), Throws.Nothing);
        }

        // ── CanvasGroup ──

        /// <summary>
        /// Open() 后 CanvasGroup 属性不应为 null。
        /// </summary>
        [Test]
        public void CanvasGroup_AfterOpen_NotNull()
        {
            _panel!.Open(_parentGo!.transform);

            Assert.That(_panel.CanvasGroup, Is.Not.Null);
        }

        /// <summary>
        /// Open() 后 CanvasGroup 应默认允许射线检测。
        /// </summary>
        [Test]
        public void CanvasGroup_AfterOpen_BlocksRaycasts_True()
        {
            _panel!.Open(_parentGo!.transform);

            Assert.That(_panel.CanvasGroup.blocksRaycasts, Is.True);
        }

        // ── RectTransform ──

        /// <summary>
        /// RectTransform 属性不应为 null。
        /// </summary>
        [Test]
        public void RectTransform_NotNull()
        {
            Assert.That(_panel!.RectTransform, Is.Not.Null);
        }

        // ── 测试用具象子类 ──

        /// <summary>
        /// UIPanel 的具象测试子类，追踪虚方法调用。
        /// </summary>
        public class ConcretePanel : UIPanel
        {
            /// <summary>OnOpen() 是否被调用。</summary>
            public bool OnOpenCalled { get; private set; }

            /// <summary>OnClose() 是否被调用。</summary>
            public bool OnCloseCalled { get; private set; }

            /// <inheritdoc />
            protected override void OnOpen()
            {
                base.OnOpen();
                OnOpenCalled = true;
            }

            /// <inheritdoc />
            protected override void OnClose()
            {
                base.OnClose();
                OnCloseCalled = true;
            }
        }
    }
}
