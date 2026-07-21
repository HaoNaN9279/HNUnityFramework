#nullable enable

using HN.Framework.Unity.Capability.Input;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace HN.Framework.Unity.Tests.Input
{
    /// <summary>
    /// DeviceDetector EditMode 测试。
    /// 使用 InputTestFixture 模拟输入设备。
    /// </summary>
    [TestFixture]
    public class DeviceDetectorTests : InputTestFixture
    {
        private DeviceDetector? _detector;

        public override void Setup()
        {
            base.Setup();
            _detector = new DeviceDetector();
        }

        public override void TearDown()
        {
            _detector?.Dispose();
            _detector = null;

            // Remove all devices added during the test
            foreach (var device in InputSystem.devices)
            {
                if (device.added)
                {
                    InputSystem.RemoveDevice(device);
                }
            }

            base.TearDown();
        }

        [Test]
        public void Constructor_WithNoDevices_ReturnsUnknown()
        {
            Assert.That(_detector!.CurrentControlScheme, Is.EqualTo("Unknown"));
        }

        [Test]
        public void Detect_WithKeyboard_ReturnsKeyboardMouse()
        {
            InputSystem.AddDevice<Keyboard>();
            string scheme = _detector!.Detect();
            Assert.That(scheme, Is.EqualTo("KeyboardMouse"));
        }

        [Test]
        public void Detect_WithGamepad_ReturnsGamepad()
        {
            InputSystem.AddDevice<Gamepad>();
            string scheme = _detector!.Detect();
            Assert.That(scheme, Is.EqualTo("Gamepad"));
        }

        [Test]
        public void Detect_WithTouchscreen_ReturnsTouch()
        {
            InputSystem.AddDevice<Touchscreen>();
            string scheme = _detector!.Detect();
            Assert.That(scheme, Is.EqualTo("Touch"));
        }

        [Test]
        public void Detect_DeviceChange_FiresEvent()
        {
            string? receivedScheme = null;
            _detector!.OnControlSchemeChanged += scheme => receivedScheme = scheme;

            InputSystem.AddDevice<Gamepad>();

            Assert.That(receivedScheme, Is.Not.Null, "Expected event to fire after device added.");
            Assert.That(receivedScheme, Is.EqualTo("Gamepad"));
        }

        [Test]
        public void Dispose_UnsubscribesFromEvents()
        {
            string? receivedScheme = null;
            _detector!.OnControlSchemeChanged += scheme => receivedScheme = scheme;

            _detector.Dispose();

            InputSystem.AddDevice<Gamepad>();

            Assert.That(receivedScheme, Is.Null, "After Dispose, event should not fire.");
        }
    }
}
