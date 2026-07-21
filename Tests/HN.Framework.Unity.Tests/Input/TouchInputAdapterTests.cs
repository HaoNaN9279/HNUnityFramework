#nullable enable

using HN.Framework.Unity.Capability.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

namespace HN.Framework.Unity.Tests.Input
{
    /// <summary>
    /// TouchInputAdapter EditMode 测试。
    /// 验证虚拟控件创建、参数验证和资源清理。
    /// </summary>
    [TestFixture]
    public class TouchInputAdapterTests
    {
        private TouchInputAdapter? _adapter;
        private Canvas? _canvas;
        private bool _previousLogEnabled;

        [SetUp]
        public void SetUp()
        {
            // OnScreenStick/OnScreenButton components log errors in EditMode
            // when controlPath references an unrecognized InputAction path.
            // Suppress these expected framework-level errors during test execution.
            _previousLogEnabled = UnityEngine.Debug.unityLogger.logEnabled;
            UnityEngine.Debug.unityLogger.logEnabled = false;

            _adapter = new TouchInputAdapter();

            var canvasGo = new GameObject("TestCanvas", typeof(Canvas));
            canvasGo.hideFlags = HideFlags.HideAndDontSave;
            _canvas = canvasGo.GetComponent<Canvas>();
        }

        [TearDown]
        public void TearDown()
        {
            _adapter?.DestroyAll();
            _adapter = null;

            if (_canvas != null)
            {
                Object.DestroyImmediate(_canvas.gameObject);
                _canvas = null;
            }

            UnityEngine.Debug.unityLogger.logEnabled = _previousLogEnabled;
        }

        [Test]
        public void CreateStick_ReturnsGameObjectWithOnScreenStick()
        {
            var go = _adapter!.CreateVirtualStick("Gameplay/Move", (RectTransform)_canvas!.transform);
            Assert.That(go, Is.Not.Null);
            Assert.That(go.GetComponent<OnScreenStick>(), Is.Not.Null);
        }

        [Test]
        public void CreateButton_ReturnsGameObjectWithOnScreenButton()
        {
            var go = _adapter!.CreateVirtualButton("Gameplay/Jump", (RectTransform)_canvas!.transform);
            Assert.That(go, Is.Not.Null);
            Assert.That(go.GetComponent<OnScreenButton>(), Is.Not.Null);
        }

        [Test]
        public void DestroyAll_CleansUpAllCreatedObjects()
        {
            var stick = _adapter!.CreateVirtualStick("Gameplay/Move", (RectTransform)_canvas!.transform);
            var button = _adapter!.CreateVirtualButton("Gameplay/Jump", (RectTransform)_canvas!.transform);

            _adapter.DestroyAll();

            Assert.That(stick == null || stick.hideFlags != HideFlags.None, "Stick should be destroyed.");
            Assert.That(button == null || button.hideFlags != HideFlags.None, "Button should be destroyed.");
        }

        [Test]
        public void CreateStick_NullParent_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _adapter!.CreateVirtualStick("Gameplay/Move", null!),
                Throws.ArgumentNullException
            );
        }

        [Test]
        public void CreateButton_NullParent_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _adapter!.CreateVirtualButton("Gameplay/Jump", null!),
                Throws.ArgumentNullException
            );
        }
    }
}
